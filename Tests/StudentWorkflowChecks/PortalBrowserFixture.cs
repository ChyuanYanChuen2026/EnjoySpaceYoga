using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Models.Enums;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// 僅由隔離測試的 --ui 模式使用，不提供日常操作入口。
internal sealed record PortalBrowserFixture(string TrialAccount, string MakeupAccount, string Password, long TrialId, long EntitlementId)
{
    public static async Task<PortalBrowserFixture> CreateAsync(Func<AppDbContext> createDb, long admin)
    {
        await using var db = createDb();
        var name = db.Database.GetDbConnection().Database;
        if (!name.StartsWith("EnjoySpace_StudentWorkflowChecks_", StringComparison.Ordinal))
            throw new InvalidOperationException("瀏覽器測試只允許使用學生流程測試的隨機資料庫。");
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.ToOffset(TimeSpan.FromHours(8)).DateTime);
        var password = "Isolated-" + Guid.NewGuid().ToString("N") + "!";
        User User(string account) {
            var user = new User { Account = account, DisplayName = account == "browser-trial" ? "隔離體驗學生" : "隔離補課學生", Phone = "0912345678", Email = account + "@example.test" };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
            return user;
        }
        var studentRole = await db.Roles.SingleAsync(x => x.RoleCode == "Student");
        var trial = new Student { User = User("browser-trial") };
        var makeup = new Student { User = User("browser-makeup") };
        db.AddRange(trial, makeup, new UserRole { User = trial.User, Role = studentRole }, new UserRole { User = makeup.User, Role = studentRole });
        var teacher = await db.Teachers.FirstAsync();
        var location = await db.StudioLocations.FirstAsync();
        Term Term(string label) => new() { Name = label, StartDate = today, EndDate = today.AddDays(60), Status = "Open", ScheduleConfirmedAtUtc = now.UtcDateTime };
        var term = Term("隔離原課程學期");
        var otherTerm = Term("隔離跨學期目標");
        var category = new ClassCategory { Code = "UI-ISOLATED", Name = "隔離驗證" };
        Course Course(string label, Term period) => new() { Name = label, Term = period, ClassCategory = category, DefaultTeacher = teacher, DefaultLocation = location, DefaultCapacity = 10, TermPrice = 1000, IsPubliclyVisible = true, AllowsMakeup = true };
        var sourceCourse = Course("隔離原固定班", term);
        var targetCourse = Course("隔離補課與體驗班", otherTerm);
        ClassSession Session(Course course, int day) {
            var start = new DateTimeOffset(today.AddDays(day).ToDateTime(new TimeOnly(18, 0)), TimeSpan.FromHours(8));
            return new() { Course = course, Teacher = teacher, StudioLocation = location, StartsAt = start, EndsAt = start.AddHours(1), Capacity = 10 };
        }
        var source = Session(sourceCourse, 1);
        var targets = Enumerable.Range(2, 4).Select(day => Session(targetCourse, day)).ToArray();
        var enrollment = new CourseEnrollment { Student = makeup, Course = sourceCourse, CreatedByUserId = admin, EnrolledAtUtc = now.UtcDateTime,
            EnrollmentApplication = new() { Student = makeup, Course = sourceCourse, Status = EnrollmentApplicationStatus.Approved, QuotedSessionCount = 1, QuotedAmount = 1000 } };
        var participant = new ClassSessionParticipant { ClassSession = source, Student = makeup, CourseEnrollment = enrollment, AddedByUserId = admin, AddedAtUtc = now.UtcDateTime };
        var plan = new LessonPlan { PlanCode = "UI-PAID", PlanName = "隔離三堂體驗", PlanType = "Trial", AudienceType = "TrialGuest", SessionQuota = 3, Price = 1000, ValidityUnit = "Days", ValidityValue = 60 };
        var request = new PortalTrialRequest { User = trial.User, TrialGuest = new() { Name = trial.User.DisplayName, Phone = "0912345678", SourceType = "Direct" }, ClassSession = targets[0], LessonPlan = plan,
            PlanName = plan.PlanName, QuotedPrice = plan.Price, QuotedQuota = 3, QuotedValidityUnit = "Days", QuotedValidityValue = 60, RequiresReferrerAtRequest = false, SubmissionKey = Guid.NewGuid(), CreatedAtUtc = now.UtcDateTime };
        db.AddRange(targets); db.AddRange(participant, request);
        await db.SaveChangesAsync();
        var leave = await new LeaveMakeupService(db, TimeProvider.System).GetLeaveFormAsync(participant.SessionParticipantId) ?? throw new Exception("缺少隔離課堂");
        leave.Reason = "隔離測試請假";
        var leaveId = await new LeaveMakeupService(db, TimeProvider.System).CreateLeaveAsync(leave, makeup.UserId, true);
        var booking = new TrialBookingService(db, TimeProvider.System);
        await booking.ReviewAsync(request.Id, request.RowVersion, true, null, today, null, admin);
        var pass = await db.LessonPasses.SingleAsync(x => x.LessonPassId == request.LessonPassId);
        await new TrialLessonService(db, new EphemeralDataProtectionProvider(), TimeProvider.System).ConfirmPaymentAsync(new() {
            Id = pass.LessonPassId, RowVersion = pass.RowVersion, ReceivedAmount = 1000, Method = "Cash", PaidAtTaiwan = now.ToOffset(TimeSpan.FromHours(8)).DateTime, ConfirmDetails = true
        }, admin);
        return new(trial.User.Account, makeup.User.Account, password, request.Id,
            await db.MakeupEntitlements.Where(x => x.SourceLeaveRequestId == leaveId).Select(x => x.MakeupEntitlementId).SingleAsync());
    }

    public async Task VerifyAsync(Func<AppDbContext> createDb, Action<bool, string> check)
    {
        await using var db = createDb();
        check(await db.MakeupRequests.CountAsync(x => x.MakeupEntitlementId == EntitlementId && x.Status == "Cancelled") >= 1, "browser makeup submit and cancel preserved history");
        check(await db.MakeupEntitlements.AnyAsync(x => x.MakeupEntitlementId == EntitlementId && x.Status == "Available"), "browser cancellation released makeup entitlement");
        check(await db.TrialBookings.CountAsync(x => x.TrialRequestId == TrialId && x.Status == "Cancelled") >= 3, "browser multi/single booking and cancellation preserved history");
        var parent = await new TrialBookingService(db, TimeProvider.System).Requests.SingleAsync(x => x.Id == TrialId);
        check(await new TrialBookingService(db, TimeProvider.System).RemainingAsync(parent) == 3, "browser cancellations restored all three trial slots");
        check(!await db.ClassSessionParticipants.AnyAsync(x => x.TrialRequestId == TrialId || x.MakeupRequest!.MakeupEntitlementId == EntitlementId), "pending browser requests never created attendance roster");
    }
}
