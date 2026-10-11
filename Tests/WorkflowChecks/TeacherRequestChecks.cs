using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EnjoySpace.Web.Migrations;

internal static class TeacherRequestChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, long admin, long student, Action<bool, string> assert)
    {
        var now = new DateTimeOffset(2032, 1, 2, 8, 0, 0, TimeSpan.FromHours(8));
        var clock = new AttendanceTestClock(now);
        TeacherRequestService Service(AppDbContext db) => new(db, clock);
        async Task<T> Read<T>(Func<AppDbContext, Task<T>> action) { await using var db = createDb(); return await action(db); }
        async Task Do(Func<AppDbContext, Task> action) { await using var db = createDb(); await action(db); }
        async Task Reject(Func<Task> action, string text, string label)
        {
            try { await action(); }
            catch (InvalidOperationException e) when (e.Message.Contains(text)) { assert(true, label); return; }
            throw new Exception("FAIL: " + label);
        }
        long teacherId, actor, otherActor, courseId, locationId; int sessionId;
        await using (var db = createDb())
        {
            var count = await db.ClassSessions.CountAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            var generator = db.GetService<IMigrationsSqlGenerator>();
            foreach (var command in generator.Generate(new AddTeacherRequests().DownOperations)) await db.Database.ExecuteSqlRawAsync(command.CommandText);
            foreach (var command in generator.Generate(new AddTeacherRequests().UpOperations)) await db.Database.ExecuteSqlRawAsync(command.CommandText);
            await tx.CommitAsync();
            assert(await db.ClassSessions.CountAsync() == count && !await db.ClassSessions.AnyAsync(x => x.IsExtraSession), "teacher request migration preserves existing sessions as regular");
        }
        await using (var db = createDb())
        {
            var role = new Role { RoleCode = "Teacher", RoleName = "老師" };
            var teacher = new Teacher { User = new User { Account = "teacher-requests", DisplayName = "申請老師", PasswordHash = "test-only" } };
            var other = new Teacher { User = new User { Account = "other-teacher-requests", DisplayName = "其他老師", PasswordHash = "test-only" } };
            db.AddRange(teacher, other);
            db.UserRoles.AddRange(new UserRole { User = teacher.User, Role = role }, new UserRole { User = other.User, Role = role });
            var term = new Term { Name = "老師申請學期", StartDate = new(2032, 1, 1), EndDate = new(2032, 12, 31), Status = "Open", ScheduleConfirmedAtUtc = now.UtcDateTime };
            var location = new StudioLocation { Name = "申請測試教室", Address = "test" };
            var course = new Course { Name = "申請測試課程", Term = term, DefaultTeacher = teacher, DefaultLocation = location,
                ClassCategoryId = await db.ClassCategories.Select(x => x.ClassCategoryId).FirstAsync(), TermPrice = 100, DefaultCapacity = 10 };
            var session = new ClassSession { Course = course, Teacher = teacher, StudioLocation = location, StartsAt = now.AddDays(2), EndsAt = now.AddDays(2).AddHours(1), Capacity = 10 };
            db.ClassSessions.Add(session); await db.SaveChangesAsync();
            teacherId = teacher.TeacherId; actor = teacher.UserId; otherActor = other.UserId; courseId = course.CourseId; locationId = location.StudioLocationId; sessionId = session.Id;
        }
        TeacherRequestInput Extra(int days = 3) => new() { TeacherId = teacherId, CourseId = courseId, StudioLocationId = locationId,
            StartsAtTaiwan = now.AddDays(days).DateTime, EndsAtTaiwan = now.AddDays(days).AddHours(1).DateTime, RequestedCapacity = 5, Reason = "加開測試" };
        TeacherRequestInput Capacity() => new() { Kind = "Capacity", TeacherId = teacherId, TargetSessionId = sessionId, RequestedCapacity = 12, Reason = "超收測試" };
        async Task<TeacherRequestReview> Review(long id, string decision = "Approve") => new() { Id = id, Decision = decision, ConfirmDetails = true,
            RowVersion = (await Read(db => db.TeacherRequests.AsNoTracking().SingleAsync(x => x.Id == id))).RowVersion, Note = "審核測試" };
        var studentActor = await Read(db => db.Students.Where(x => x.StudentId == student).Select(x => x.UserId).SingleAsync());
        await Reject(() => Do(db => Service(db).SubmitAsync(Extra(), studentActor)), "老師或管理員", "student cannot submit teacher requests");
        await Reject(() => Do(db => Service(db).SubmitAsync(Extra(), otherActor)), "自己", "teacher cannot impersonate another teacher");
        var bad = Extra(); bad.Reason = " ";
        await Reject(() => Do(db => Service(db).SubmitAsync(bad, actor)), "必填", "request requires reason");
        bad = Extra(); bad.RequestedCapacity = 0;
        await Reject(() => Do(db => Service(db).SubmitAsync(bad, actor)), "名額", "request rejects zero capacity");
        await Reject(() => Do(db => Service(db).SubmitAsync(Extra(2), actor)), "衝突", "extra session checks teacher and room conflicts");
        await Reject(() => Do(db => Service(db).SubmitAsync(Extra(400), actor)), "同一期", "extra session cannot cross term");
        await Reject(() => Do(db => Service(db).SubmitAsync(Extra(-1), actor)), "尚未開始", "past extra session rejected");
        var input = Extra(); var id = await Read(db => Service(db).SubmitAsync(input, actor));
        assert(await Read(db => Service(db).SubmitAsync(input, actor)) == id, "teacher request replay is idempotent");
        assert(await Read(db => db.ClassSessions.CountAsync(x => x.CourseId == courseId)) == 1, "pending extra request does not create a session");
        await Reject(() => Do(db => Service(db).SubmitAsync(Extra(), actor)), "待審", "duplicate pending extra request rejected");
        assert(await Read(db => Service(db).DetailAsync(id, otherActor)) == null, "teacher cannot read another teacher request");
        assert((await Read(db => Service(db).ListAsync(otherActor, 1))).TotalCount == 0, "teacher list only shows own requests");
        var review = await Review(id);
        await Reject(() => Do(db => Service(db).ReviewAsync(review, actor)), "管理員", "teacher cannot approve request");
        review.ConfirmDetails = false;
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "勾選", "review requires confirmation");
        review.ConfirmDetails = true;
        await Do(db => Service(db).ReviewAsync(review, admin));
        var extraId = await Read(db => db.TeacherRequests.Where(x => x.Id == id).Select(x => x.CreatedSessionId!.Value).SingleAsync());
        assert(await Read(db => db.ClassSessions.AnyAsync(x => x.Id == extraId && x.IsExtraSession && x.Capacity == 5)), "approval creates marked extra session with requested capacity");
        assert(!await Read(db => db.ClassSessionParticipants.AnyAsync(x => x.ClassSessionId == extraId)), "extra session does not auto enroll fixed students");
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "已變更", "repeated review cannot duplicate extra session");
        var protection = new EphemeralDataProtectionProvider();
        var preview = await Read(db => new EnrollmentWorkflowService(db, protection).PreviewAsync(student, courseId, admin));
        assert(preview.Sessions.Count == 1 && preview.Amount == 100 && preview.Sessions[0].Id == sessionId, "extra session excluded from regular quote and price");
        var appId = await Read(db => new EnrollmentWorkflowService(db, protection).CreateAsync(student, courseId, admin, preview.ConfirmationToken, true, true));
        var app = await Read(db => db.EnrollmentApplications.AsNoTracking().SingleAsync(x => x.Id == appId));
        await Do(db => new EnrollmentWorkflowService(db, protection).ConfirmPaymentAsync(appId, app.RowVersion, 100, "Cash", DateTime.UtcNow.AddHours(8), null, admin, true));
        app = await Read(db => db.EnrollmentApplications.AsNoTracking().SingleAsync(x => x.Id == appId));
        await Do(db => new EnrollmentWorkflowService(db, protection).ReviewAsync(appId, app.RowVersion, true, null, admin, true));
        assert(await Read(db => db.ClassSessionParticipants.CountAsync(x => x.CourseEnrollment.EnrollmentApplicationId == appId)) == 1, "regular enrollment creates roster only for regular sessions");
        var cap = Capacity(); var capId = await Read(db => Service(db).SubmitAsync(cap, actor));
        assert(await Read(db => db.ClassSessions.Where(x => x.Id == sessionId).Select(x => x.CapacityOverride).SingleAsync()) == null, "pending capacity request leaves limit unchanged");
        await Reject(() => Do(db => Service(db).SubmitAsync(Capacity(), actor)), "待審", "one pending capacity request per session");
        review = await Review(capId); review.Decision = "Reject"; review.Note = " ";
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "原因", "rejection requires reason");
        review.Note = "空間不足"; await Do(db => Service(db).ReviewAsync(review, admin));
        assert(await Read(db => db.ClassSessions.Where(x => x.Id == sessionId).Select(x => x.CapacityOverride).SingleAsync()) == null, "rejection leaves capacity unchanged");
        capId = await Read(db => Service(db).SubmitAsync(Capacity(), admin)); review = await Review(capId);
        await Do(async db => { var s = await db.ClassSessions.FindAsync(sessionId); s!.Capacity = 11; await db.SaveChangesAsync(); });
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "課堂資料", "approval rejects changed target session version");
        review.Decision = "Reject"; await Do(db => Service(db).ReviewAsync(review, admin));
        capId = await Read(db => Service(db).SubmitAsync(Capacity(), actor)); review = await Review(capId);
        await Do(db => Service(db).ReviewAsync(review, admin));
        assert(await Read(db => db.ClassSessions.AnyAsync(x => x.Id == sessionId && x.CapacityOverride == 12 && x.Capacity == 11)), "approved capacity updates effective limit only");
        assert(await Read(db => db.Courses.Where(x => x.CourseId == courseId).Select(x => x.DefaultCapacity).SingleAsync()) == 10, "single session capacity does not raise term enrollment limit");
        await Reject(() => Do(db => Service(db).SubmitAsync(Capacity(), actor)), "大於", "capacity request must increase current limit");
        id = await Read(db => Service(db).SubmitAsync(Extra(6), actor)); review = await Review(id);
        await Do(async db => { db.ClassSessions.Add(new ClassSession { CourseId = courseId, TeacherId = teacherId, StudioLocationId = locationId,
            StartsAt = now.AddDays(6), EndsAt = now.AddDays(6).AddHours(1), Capacity = 10 }); await db.SaveChangesAsync(); });
        await Reject(() => Do(db => Service(db).ReviewAsync(review, admin)), "衝突", "approval rechecks conflicts introduced after submission");
        id = await Read(db => Service(db).SubmitAsync(Extra(7), actor)); review = await Review(id);
        await Reject(() => Do(db => new TeacherRequestService(db, new AttendanceTestClock(now.AddDays(8))).ReviewAsync(review, admin)), "尚未開始", "approval rejects request whose start time passed");
        id = await Read(db => Service(db).SubmitAsync(Extra(8), actor)); review = await Review(id);
        async Task<bool> ApproveConcurrently()
        {
            try { await Do(db => Service(db).ReviewAsync(review, admin)); return true; }
            catch (InvalidOperationException) { return false; }
            catch (DbUpdateException) { return false; }
            catch (SqlException e) when (e.Number == 1205) { return false; }
        }
        var results = await Task.WhenAll(ApproveConcurrently(), ApproveConcurrently());
        assert(results.Count(x => x) == 1 && await Read(db => db.ClassSessions.CountAsync(x => x.CourseId == courseId && x.StartsAt == now.AddDays(8))) == 1,
            "concurrent reviews create only one extra session");
        await Do(async db => { var t = await db.Teachers.FindAsync(teacherId); t!.IsActive = false; await db.SaveChangesAsync(); });
        assert((await Read(db => StaffAccess.RolesAsync(db, actor))).Count == 0, "disabled teacher loses staff access");
        await Do(async db => { var t = await db.Teachers.FindAsync(teacherId); t!.IsActive = true; await db.SaveChangesAsync(); });
    }
}
