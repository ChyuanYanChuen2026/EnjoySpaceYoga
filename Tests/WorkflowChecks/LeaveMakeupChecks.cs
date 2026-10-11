using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

internal static class LeaveMakeupChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, long admin, long student, long course, long targetCourse, Action<bool, string> assert)
    {
        var clock = new AttendanceTestClock(new DateTimeOffset(2029, 12, 1, 0, 0, 0, TimeSpan.Zero));
        var lateClock = new AttendanceTestClock(new DateTimeOffset(2030, 4, 1, 0, 0, 0, TimeSpan.Zero));
        async Task<T> Read<T>(Func<AppDbContext, Task<T>> action) { await using var db = createDb(); return await action(db); }
        async Task Do(Func<AppDbContext, Task> action) { await using var db = createDb(); await action(db); }
        LeaveMakeupService Service(AppDbContext db) => new(db, clock);
        async Task Reject(Func<Task> action, string contains, string label)
        {
            try { await action(); }
            catch (InvalidOperationException e) when (e.Message.Contains(contains)) { assert(true, label); return; }
            throw new Exception("FAIL: " + label);
        }
        await Do(async db =>
        {
            var enrollment = await db.CourseEnrollments.SingleAsync(x => x.CourseId == course && x.StudentId == student);
            enrollment.Status = "Active";
            await db.SaveChangesAsync();
        });
        var sourceIds = await Read(db => db.ClassSessionParticipants.Where(x => x.StudentId == student).OrderBy(x => x.ClassSessionId).Select(x => x.SessionParticipantId).ToListAsync());
        var target = await Read(db => db.ClassSessions.SingleAsync(x => x.CourseId == targetCourse));
        async Task<LeaveCreateModel> LeaveForm(long id) => (await Read(db => Service(db).GetLeaveFormAsync(id)))!;
        async Task<LeaveMakeupActionModel> LeaveAction(long id) => await Read(db => db.LeaveRequests.Where(x => x.LeaveRequestId == id)
            .Select(x => new LeaveMakeupActionModel { Id = x.LeaveRequestId, RowVersion = x.RowVersion }).SingleAsync());
        async Task<LeaveMakeupActionModel> RequestAction(long id, bool approve = true) => await Read(db => db.MakeupRequests.Where(x => x.MakeupRequestId == id)
            .Select(x => new LeaveMakeupActionModel { Id = x.MakeupRequestId, RowVersion = x.RowVersion, Approve = approve, Note = "流程測試" }).SingleAsync());
        async Task<MakeupCreateModel> MakeupForm(long leaveId) => (await Read(async db => await Service(db).GetMakeupFormAsync(
            await db.MakeupEntitlements.Where(x => x.SourceLeaveRequestId == leaveId).Select(x => x.MakeupEntitlementId).SingleAsync())))!;
        async Task<long> Leave(long participantId)
        {
            var input = await LeaveForm(participantId); input.Reason = "測試請假";
            return await Read(db => Service(db).CreateLeaveAsync(input, admin));
        }
        async Task<long> Apply(long leaveId)
        {
            var input = await MakeupForm(leaveId); input.TargetSessionId = target.Id;
            return await Read(db => Service(db).CreateMakeupAsync(input, admin));
        }
        var first = await LeaveForm(sourceIds[0]); first.Reason = "測試請假";
        await Reject(() => Do(db => Service(db).CreateLeaveAsync(first, 0)), "管理員", "leave requires admin");
        await Reject(() => Do(db => new LeaveMakeupService(db, lateClock).CreateLeaveAsync(first, admin)), "已開始", "leave after class start rejected");
        var invalid = await LeaveForm(sourceIds[0]); invalid.Reason = " ";
        await Reject(() => Do(db => Service(db).CreateLeaveAsync(invalid, admin)), "原因", "leave requires reason");
        var firstLeave = await Read(db => Service(db).CreateLeaveAsync(first, admin));
        assert(await Read(db => db.ClassSessionParticipants.AnyAsync(x => x.SessionParticipantId == first.ParticipantId && x.Status == "OnLeave")), "leave releases original seat");
        assert(await Read(db => db.MakeupEntitlements.CountAsync(x => x.SourceLeaveRequestId == firstLeave && x.Status == "Available")) == 1, "leave creates exactly one entitlement");
        await Reject(() => Do(db => Service(db).CreateLeaveAsync(first, admin)), "已變更", "duplicate leave form rejected");
        var firstCancel = await LeaveAction(firstLeave);
        await Reject(() => Do(db => new LeaveMakeupService(db, lateClock).CancelLeaveAsync(firstCancel, admin)), "已開始", "leave cannot be cancelled after start");
        await Do(db => Service(db).CancelLeaveAsync(firstCancel, admin));
        assert(await Read(db => db.ClassSessionParticipants.AnyAsync(x => x.SessionParticipantId == first.ParticipantId && x.Status == "Active")), "cancel leave restores regular roster");
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == firstLeave && x.Status == "Revoked")), "cancel leave revokes entitlement");
        var leave = await Leave(sourceIds[0]);
        var form = await MakeupForm(leave);
        assert(form.Targets.Any(x => x.Value == target.Id.ToString()), "makeup form offers eligible target");
        var ownSession = await Read(db => db.ClassSessionParticipants.Where(x => x.SessionParticipantId == sourceIds[1]).Select(x => x.ClassSessionId).SingleAsync());
        form.TargetSessionId = ownSession;
        await Reject(() => Do(db => Service(db).CreateMakeupAsync(form, admin)), "名單", "cannot makeup in own regular class");
        var otherTermId = await Read(async db =>
        {
            var t = new Term { Name = "不同期", StartDate = new(2030, 1, 1), EndDate = new(2030, 12, 31) };
            db.Terms.Add(t); await db.SaveChangesAsync(); return t.TermId;
        });
        var originalTerm = await Read(db => db.Courses.Where(x => x.CourseId == targetCourse).Select(x => x.TermId).SingleAsync());
        await Do(async db => { var c = await db.Courses.FindAsync(targetCourse); c!.TermId = otherTermId; await db.SaveChangesAsync(); });
        form.TargetSessionId = target.Id;
        await Reject(() => Do(db => Service(db).CreateMakeupAsync(form, admin)), "同一期", "cross-term makeup rejected");
        await Do(async db => { var c = await db.Courses.FindAsync(targetCourse); c!.TermId = originalTerm; c.AllowsMakeup = false; await db.SaveChangesAsync(); });
        await Reject(() => Do(db => Service(db).CreateMakeupAsync(form, admin)), "未開放", "disabled makeup course rejected");
        await Do(async db => { var c = await db.Courses.FindAsync(targetCourse); c!.AllowsMakeup = true; await db.SaveChangesAsync(); });
        var request = await Read(db => Service(db).CreateMakeupAsync(form, admin));
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == leave && x.Status == "Reserved")), "pending application reserves entitlement");
        assert(!await Read(db => db.ClassSessionParticipants.AnyAsync(x => x.MakeupRequestId == request)), "pending makeup does not occupy a seat");
        await Reject(() => Do(db => Service(db).CreateMakeupAsync(form, admin)), "已變更", "one entitlement cannot be submitted twice");
        var cancelLeave = await LeaveAction(leave);
        await Reject(() => Do(db => Service(db).CancelLeaveAsync(cancelLeave, admin)), "先取消", "pending makeup blocks original leave cancellation");
        var review = await RequestAction(request, false); review.Note = "";
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "原因", "reject makeup requires reason");
        review.Note = "測試退回";
        await Do(db => Service(db).ReviewAsync(review, admin));
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == leave && x.Status == "Available")), "rejection returns entitlement");
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "已變更", "stale review rejected");
        request = await Apply(leave);
        review = await RequestAction(request);
        await Reject(() => Do(db => Service(db).ReviewAsync(review, 0)), "管理員", "makeup approval requires admin");
        await Do(db => Service(db).ReviewAsync(review, admin));
        assert(await Read(db => db.ClassSessionParticipants.CountAsync(x => x.MakeupRequestId == request && x.Status == "Active" && x.ParticipationType == "Makeup")) == 1, "approval creates one makeup participant");
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == leave && x.Status == "Consumed")), "approval consumes entitlement");
        var makeupParticipant = await Read(db => db.ClassSessionParticipants.SingleAsync(x => x.MakeupRequestId == request));
        var makeupLeave = await LeaveForm(makeupParticipant.SessionParticipantId); makeupLeave.Reason = "再次請假";
        await Reject(() => Do(db => Service(db).CreateLeaveAsync(makeupLeave, admin)), "固定期課", "makeup cannot generate another entitlement");
        cancelLeave = await LeaveAction(leave);
        await Reject(() => Do(db => Service(db).CancelLeaveAsync(cancelLeave, admin)), "先取消", "consumed entitlement blocks leave cancellation");
        var cancelMakeup = await RequestAction(request);
        await Reject(() => Do(db => new LeaveMakeupService(db, lateClock).CancelMakeupAsync(cancelMakeup, admin)), "已開始", "approved makeup cannot be cancelled after start");
        await Do(db => Service(db).CancelMakeupAsync(cancelMakeup, admin));
        assert(await Read(db => db.ClassSessionParticipants.AnyAsync(x => x.MakeupRequestId == request && x.Status == "Cancelled")), "cancel makeup preserves inactive participant");
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == leave && x.Status == "Available" && x.ConsumedAtUtc == null)), "cancel makeup returns qualification");
        request = await Apply(leave); review = await RequestAction(request);
        await Do(db => Service(db).ReviewAsync(review, admin));
        assert(await Read(db => db.ClassSessionParticipants.CountAsync(x => x.ClassSessionId == target.Id && x.StudentId == student && x.Status == "Active")) == 1, "reapply same target after cancellation creates no duplicate active seat");
        cancelMakeup = await RequestAction(request); await Do(db => Service(db).CancelMakeupAsync(cancelMakeup, admin));
        cancelLeave = await LeaveAction(leave); await Do(db => Service(db).CancelLeaveAsync(cancelLeave, admin));
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == leave && x.Status == "Revoked")), "cancel makeup then leave revokes qualification");

        // Two different students compete for the final target seat.
        long otherParticipant = 0;
        await Do(async db =>
        {
            var p = await db.ClassSessionParticipants.Include(x => x.CourseEnrollment).SingleAsync(x => x.SessionParticipantId == sourceIds[1]);
            var other = new Student { User = new User { Account = "makeup-other", DisplayName = "補課學生乙", PasswordHash = "test-only" } };
            var application = new EnrollmentApplication { Student = other, CourseId = course, Status = EnjoySpace.Web.Models.Enums.EnrollmentApplicationStatus.Approved,
                QuotedSessionCount = 10, QuotedAmount = 100 };
            var enrollment = new CourseEnrollment { Student = other, CourseId = course, EnrollmentApplication = application, CreatedByUserId = admin };
            var participant2 = new ClassSessionParticipant { Student = other, CourseEnrollment = enrollment, ClassSessionId = p.ClassSessionId, AddedByUserId = admin };
            db.Add(participant2); (await db.ClassSessions.FindAsync(target.Id))!.Capacity = 1;
            await db.SaveChangesAsync(); otherParticipant = participant2.SessionParticipantId;
        });
        var leaveA = await Leave(sourceIds[1]); var leaveB = await Leave(otherParticipant);
        var requestA = await Apply(leaveA); var requestB = await Apply(leaveB);
        var reviewA = await RequestAction(requestA); var reviewB = await RequestAction(requestB);
        async Task<bool> Compete(LeaveMakeupActionModel input)
        {
            try { await Do(db => Service(db).ReviewAsync(input, admin)); return true; }
            catch (InvalidOperationException e) when (e.Message.Contains("已滿")) { return false; }
            catch (SqlException e) when (e.Number == 1205) { return false; }
        }
        var results = await Task.WhenAll(Compete(reviewA), Compete(reviewB));
        assert(results.Count(x => x) == 1, "concurrent approvals cannot oversell last seat");
        assert(await Read(db => db.ClassSessionParticipants.CountAsync(x => x.ClassSessionId == target.Id && x.Status == "Active")) == 1, "target capacity holds after concurrent requests");
        var loser = await Read(db => db.MakeupRequests.Where(x => x.Status == "Pending").OrderByDescending(x => x.MakeupRequestId).FirstAsync());
        var loserAction = await RequestAction(loser.MakeupRequestId);
        await Reject(() => Do(db => Service(db).ReviewAsync(loserAction, admin)), "已滿", "approval rechecks capacity after request submission");
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.MakeupEntitlementId == loser.MakeupEntitlementId && x.Status == "Reserved")), "failed approval preserves pending entitlement");
        await Do(db => Service(db).CancelMakeupAsync(loserAction, admin));
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.MakeupEntitlementId == loser.MakeupEntitlementId && x.Status == "Available")), "pending cancellation releases qualification");
        assert((await Read(db => Service(db).GetListAsync("requests", int.MaxValue))).Page == 1, "management list clamps pagination");
        assert((await Read(db => Service(db).GetListAsync("leaves", 1))).Leaves.Count > 0, "leave management list loads relationships");
        assert((await Read(db => Service(db).GetListAsync("entitlements", 1))).Entitlements.Count > 0, "entitlement management list loads relationships");

        // A regular enrollment must respect seats already occupied by approved makeup.
        var protection = new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider();
        var newcomer = await Read(async db =>
        {
            var s = new Student { User = new User { Account = "newcomer-check", DisplayName = "新報名學生", PasswordHash = "test-only" } };
            db.Students.Add(s); await db.SaveChangesAsync(); return s.StudentId;
        });
        var preview = await Read(db => new EnrollmentWorkflowService(db, protection).PreviewAsync(newcomer, targetCourse, admin));
        var applicationId = await Read(db => new EnrollmentWorkflowService(db, protection).CreateAsync(newcomer, targetCourse, admin, preview.ConfirmationToken, true, true));
        await Do(async db =>
        {
            var a = await db.EnrollmentApplications.SingleAsync(x => x.Id == applicationId);
            await new EnrollmentWorkflowService(db, protection).ConfirmPaymentAsync(a.Id, a.RowVersion, preview.Amount, "Cash", DateTime.UtcNow.AddHours(8), null, admin, true);
        });
        await Reject(() => Do(async db =>
        {
            var a = await db.EnrollmentApplications.SingleAsync(x => x.Id == applicationId);
            await new EnrollmentWorkflowService(db, protection).ReviewAsync(a.Id, a.RowVersion, true, null, admin, true);
        }), "已滿", "regular enrollment cannot oversell a makeup occupied seat");
        assert(!await Read(db => db.CourseEnrollments.AnyAsync(x => x.EnrollmentApplicationId == applicationId)), "failed regular approval leaves no partial enrollment");

        var fullLeave = await Leave(sourceIds[0]);
        await Do(async db =>
        {
            var original = await db.ClassSessionParticipants.SingleAsync(x => x.SessionParticipantId == sourceIds[0]);
            var other = await db.ClassSessionParticipants.SingleAsync(x => x.SessionParticipantId == otherParticipant);
            db.ClassSessionParticipants.Add(new ClassSessionParticipant { ClassSessionId = original.ClassSessionId,
                StudentId = other.StudentId, CourseEnrollmentId = other.CourseEnrollmentId, AddedByUserId = admin });
            (await db.ClassSessions.FindAsync(original.ClassSessionId))!.Capacity = 1;
            await db.SaveChangesAsync();
        });
        var fullCancel = await LeaveAction(fullLeave);
        await Reject(() => Do(db => Service(db).CancelLeaveAsync(fullCancel, admin)), "已滿", "leave cancellation cannot overfill original class");
        assert(await Read(db => db.MakeupEntitlements.AnyAsync(x => x.SourceLeaveRequestId == fullLeave && x.Status == "Available")), "failed leave cancellation does not revoke qualification");

        // Serve fresh data to the real MVC HTTP checks without touching the development database.
        var approvedRequests = await Read(db => db.MakeupRequests.Where(x => x.Status == "Approved")
            .Select(x => new LeaveMakeupActionModel { Id = x.MakeupRequestId, RowVersion = x.RowVersion, Note = "結束名額競爭測試" }).ToListAsync());
        foreach (var approved in approvedRequests) await Do(db => Service(db).CancelMakeupAsync(approved, admin));
        await Do(async db => { (await db.ClassSessions.FindAsync(target.Id))!.Capacity = 10; await db.SaveChangesAsync(); });
    }
}
