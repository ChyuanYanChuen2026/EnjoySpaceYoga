using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Models.Enums;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using EnjoySpace.Web.Migrations;

// Always create a new isolated database; never accept an existing database name.
var database = "EnjoySpace_WorkflowChecks_" + Guid.NewGuid().ToString("N");
var connection = new SqlConnectionStringBuilder
{
    DataSource = Environment.GetEnvironmentVariable("ENJOYSPACE_TEST_SERVER") ?? "localhost",
    InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 5
};
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection.ConnectionString).Options;
var protection = new EphemeralDataProtectionProvider();
AppDbContext Db() => new(options);
EnrollmentWorkflowService Workflow(AppDbContext db) => new(db, protection);
var created = false;
var checks = 0;
void Assert(bool condition, string label)
{
    if (!condition) throw new Exception("FAIL: " + label);
    Console.WriteLine("PASS: " + label);
    checks++;
}
async Task Reject(Func<Task> action, string message, string label)
{
    try { await action(); }
    catch (InvalidOperationException e) when (e.Message.Contains(message)) { Assert(true, label); return; }
    throw new Exception("FAIL (expected rejection): " + label);
}
async Task InDb(Func<AppDbContext, Task> action) { await using var db = Db(); await action(db); }
async Task<T> Read<T>(Func<AppDbContext, Task<T>> action) { await using var db = Db(); return await action(db); }
long adminId = 0, studentId = 0, otherStudentId = 0, termId = 0, courseId = 0, emptyCourseId = 0;
long teacherId = 0, locationId = 0;
async Task ChangeTerm(string status, bool confirm = false) => await InDb(async db =>
{
    var term = await db.Terms.AsNoTracking().SingleAsync(x => x.TermId == termId);
    await new TermScheduleService(db, Workflow(db)).UpdateAsync(new TermEditViewModel
    {
        TermId = termId, Name = term.Name, StartDate = term.StartDate, EndDate = term.EndDate,
        RowVersion = term.RowVersion, Status = status, ConfirmScheduleComplete = confirm
    }, adminId);
});
try
{
    await using (var db = Db())
    {
        created = await db.Database.EnsureCreatedAsync();
        if (!created) throw new Exception("Refusing to use an existing database.");
        var admin = new User { Account = "admin-check", DisplayName = "管理員", PasswordHash = "test-only" };
        var role = new Role { RoleCode = "Admin", RoleName = "Admin" };
        db.UserRoles.Add(new UserRole { User = admin, Role = role });
        var student = new Student { User = new User { Account = "student-check", DisplayName = "學生甲", PasswordHash = "test-only" } };
        var otherStudent = new Student { User = new User { Account = "other-student", DisplayName = "學生乙", PasswordHash = "test-only" } };
        var teacher = new Teacher { User = new User { Account = "teacher-check", DisplayName = "老師", PasswordHash = "test-only" } };
        var location = new StudioLocation { Name = "測試教室", Address = "test-only" };
        var category = new ClassCategory { Code = "Check", Name = "測試" };
        var term = new Term { Name = "測試學期", StartDate = new(2030, 1, 1), EndDate = new(2030, 12, 31) };
        db.AddRange(student, otherStudent, teacher, location, category, term);
        await db.SaveChangesAsync();
        var course = new Course { Name = "完整課程", Term = term, ClassCategory = category, DefaultTeacher = teacher,
            DefaultLocation = location, TermPrice = 100, DefaultCapacity = 10 };
        var emptyCourse = new Course { Name = "未排課課程", Term = term, ClassCategory = category, DefaultTeacher = teacher,
            DefaultLocation = location, TermPrice = 200 };
        db.AddRange(course, emptyCourse);
        await db.SaveChangesAsync();
        adminId = admin.UserId; studentId = student.StudentId; otherStudentId = otherStudent.StudentId;
        termId = term.TermId; courseId = course.CourseId; emptyCourseId = emptyCourse.CourseId;
        teacherId = teacher.TeacherId; locationId = location.StudioLocationId;
    }
    for (var i = 1; i <= 11; i++)
        await InDb(async db => Assert(await new ClassSessionService(db).CreateAsync(courseId, teacherId, locationId,
            new DateTime(2030, 1, i, 9, 0, 0), new DateTime(2030, 1, i, 10, 0, 0)) is null, "create draft session " + i));
    var sessionId = await Read(db => db.ClassSessions.Where(x => x.CourseId == courseId).MaxAsync(x => x.Id));
    var staleTerm = await Read(db => db.Terms.AsNoTracking().SingleAsync(x => x.TermId == termId));
    await Reject(() => InDb(db => new ClassSessionService(db).DeleteAsync(sessionId, [0])), "已變更", "stale deletion rejected");
    await InDb(async db =>
    {
        var version = await db.ClassSessions.Where(x => x.Id == sessionId).Select(x => x.RowVersion).SingleAsync();
        await new ClassSessionService(db).DeleteAsync(sessionId, version);
    });
    Assert(await Read(db => db.ClassSessions.CountAsync(x => x.CourseId == courseId)) == 10, "draft unpurchased session deleted");
    await Reject(() => InDb(db => new TermScheduleService(db, Workflow(db)).UpdateAsync(new TermEditViewModel
    {
        TermId = termId, Name = staleTerm.Name, StartDate = staleTerm.StartDate, EndDate = staleTerm.EndDate,
        RowVersion = staleTerm.RowVersion, Status = "Open", ConfirmScheduleComplete = true
    }, adminId)), "已變更", "schedule mutation invalidates previously opened semester release form");
    await Reject(() => ChangeTerm("Open", true), "尚無有效場次", "cannot release incomplete semester");
    await InDb(async db => Assert(await new ClassSessionService(db).CreateAsync(emptyCourseId, teacherId, locationId,
        new DateTime(2030, 2, 1, 9, 0, 0), new DateTime(2030, 2, 1, 10, 0, 0)) is null, "complete remaining course"));
    await Reject(() => ChangeTerm("Open"), "勾選", "release requires explicit confirmation");
    await ChangeTerm("Open", true);
    Assert(await Read(db => db.Terms.AnyAsync(x => x.TermId == termId && x.ScheduleConfirmedByUserId == adminId)), "semester confirmation saved");
    await ChangeTerm("Closed");
    await ChangeTerm("Draft");
    Assert(await Read(db => db.Terms.AnyAsync(x => x.TermId == termId && x.ScheduleConfirmedAtUtc == null)), "unused semester can return to draft and clears confirmation");
    await ChangeTerm("Open", true);
    await InDb(async db => Assert(await new ClassSessionService(db).CreateAsync(courseId, teacherId, locationId,
        new DateTime(2030, 3, 1, 9, 0, 0), new DateTime(2030, 3, 1, 10, 0, 0)) is not null, "released schedule cannot add sessions"));
    var protectedSession = await Read(db => db.ClassSessions.AsNoTracking().FirstAsync(x => x.CourseId == courseId));
    await Reject(() => InDb(db => new ClassSessionService(db).DeleteAsync(protectedSession.Id, protectedSession.RowVersion)), "草稿", "released schedule cannot delete sessions");
    var preview = await Read(db => Workflow(db).PreviewAsync(studentId, courseId, adminId));
    Assert(preview.Sessions.Count == 10 && preview.Amount == 100, "preview includes complete schedule and server quote");
    await Reject(() => InDb(db => Workflow(db).CreateAsync(studentId, courseId, adminId, preview.ConfirmationToken, true, false)), "確認", "policy checkbox enforced on server");
    await Reject(() => InDb(db => Workflow(db).CreateAsync(studentId, courseId, adminId, "tampered", true, true)), "失效", "tampered token rejected");
    await Reject(() => InDb(db => Workflow(db).CreateAsync(otherStudentId, courseId, adminId, preview.ConfirmationToken, true, true)), "已變更", "token bound to selected student");
    await InDb(async db => { var session = await db.ClassSessions.FirstAsync(x => x.CourseId == courseId); session.StartsAt = session.StartsAt.AddMinutes(1); await db.SaveChangesAsync(); });
    await Reject(() => InDb(db => Workflow(db).CreateAsync(studentId, courseId, adminId, preview.ConfirmationToken, true, true)), "已變更", "changed schedule invalidates preview");
    preview = await Read(db => Workflow(db).PreviewAsync(studentId, courseId, adminId));
    var applicationId = await Read(db => Workflow(db).CreateAsync(studentId, courseId, adminId, preview.ConfirmationToken, true, true));
    Assert(await Read(db => db.EnrollmentApplications.AnyAsync(x => x.Id == applicationId && x.PolicyConfirmedByUserId == adminId && x.ConfirmedPolicy != null)), "student agreement audit saved");
    await Reject(() => InDb(db => Workflow(db).CreateAsync(studentId, courseId, adminId, preview.ConfirmationToken, true, true)), "已有", "duplicate confirmation does not duplicate application");
    async Task Payment(bool confirmed) => await InDb(async db =>
    {
        var version = await db.EnrollmentApplications.Where(x => x.Id == applicationId).Select(x => x.RowVersion).SingleAsync();
        await Workflow(db).ConfirmPaymentAsync(applicationId, version, 100, "Cash", DateTime.UtcNow.AddHours(8), null, adminId, confirmed);
    });
    await Reject(() => Payment(false), "再次確認", "collection requires repeated agreement");
    await Payment(true);
    async Task Review(bool confirmed) => await InDb(async db =>
    {
        var version = await db.EnrollmentApplications.Where(x => x.Id == applicationId).Select(x => x.RowVersion).SingleAsync();
        await Workflow(db).ReviewAsync(applicationId, version, true, null, adminId, confirmed);
    });
    await Reject(() => Review(false), "最後核對", "approval requires final check");
    await Review(true);
    Assert(await Read(db => db.CourseEnrollments.CountAsync(x => x.EnrollmentApplicationId == applicationId)) == 1, "approval creates one official enrollment");
    await Reject(() => InDb(async db =>
    {
        var app = await db.EnrollmentApplications.SingleAsync(x => x.Id == applicationId);
        await Workflow(db).ReviewAsync(app.Id, app.RowVersion, false, "退課", adminId, false);
    }), "已審核", "approved enrollment cannot withdraw through rejection endpoint");
    var currentSession = await Read(db => db.ClassSessions.AsNoTracking().FirstAsync(x => x.CourseId == courseId));
    await Reject(() => InDb(db => new ClassSessionService(db).DeleteAsync(currentSession.Id, currentSession.RowVersion)), "已有報名", "purchased session cannot be deleted");
    await ChangeTerm("Closed");
    await Reject(() => ChangeTerm("Draft"), "已有報名", "purchased semester cannot return to draft");
    await Reject(() => ChangeTerm("Open"), "勾選", "reopening requires confirmation again");
    await ChangeTerm("Open", true);
    await InDb(async db =>
    {
        var legacy = new EnrollmentApplication { StudentId = otherStudentId, CourseId = courseId,
            QuotedSessionCount = 9, QuotedAmount = 100, Payment = new Payment { StudentId = otherStudentId, Amount = 100 } };
        db.Add(legacy); await db.SaveChangesAsync();
    });
    await Reject(() => InDb(async db =>
    {
        var app = await db.EnrollmentApplications.SingleAsync(x => x.StudentId == otherStudentId);
        await Workflow(db).ConfirmPaymentAsync(app.Id, app.RowVersion, 100, "Cash", DateTime.UtcNow.AddHours(8), null, adminId, true);
    }), "堂數", "legacy mismatched quote cannot collect payment");
    await InDb(async db =>
    {
        var catalog = new AdminCatalogService(db);
        var page = await catalog.GetSessionsAsync(new CatalogFilter { Page = 2, Sort = "date_asc" });
        Assert(page.Items.Count == 1 && page.Controls.TotalCount == 11, "server pagination returns final page");
        var filtered = await catalog.GetSessionsAsync(new CatalogFilter { CourseId = courseId, Page = int.MaxValue });
        Assert(filtered.Items.Count == 10 && filtered.Controls.Filter.Page == 1, "filter before paging and clamp out-of-range page");
        Assert((await catalog.GetTermsAsync(new CatalogFilter { Search = "測試", Status = "Open" })).Items.Count == 1, "semester filters translate to SQL");
        Assert((await catalog.GetCoursesAsync(new CatalogFilter { Sort = "price_desc" })).Items.First().TermPrice == 200, "course price sorting translates to SQL");
    });
    Assert(await Read(db => db.ClassSessionParticipants.CountAsync()) == 10, "approval creates participants for all ten sessions only");
    Assert(await Read(db => db.AttendanceRecords.CountAsync(x => x.Status == "Scheduled")) == 10, "new participants start unmarked");
    // Simulate upgrading the preceding schema with an already-approved enrollment, only in this random isolated DB.
    await InDb(async db =>
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var generator = db.GetService<IMigrationsSqlGenerator>();
        foreach (var command in generator.Generate(new AddLeaveAndMakeupWorkflow().DownOperations))
            await db.Database.ExecuteSqlRawAsync(command.CommandText);
        await db.Database.ExecuteSqlRawAsync("DROP TABLE dbo.AttendanceRecords; DROP TABLE dbo.ClassSessionParticipants;");
        var migration = new AddSessionParticipantsAndAttendance();
        var commands = db.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations);
        foreach (var command in commands) await db.Database.ExecuteSqlRawAsync(command.CommandText);
        foreach (var command in generator.Generate(new AddLeaveAndMakeupWorkflow().UpOperations))
            await db.Database.ExecuteSqlRawAsync(command.CommandText);
        await tx.CommitAsync();
    });
    Assert(await Read(db => db.ClassSessionParticipants.CountAsync()) == 10, "migration backfills approved enrollment and excludes pending applications");
    Assert(await Read(db => db.AttendanceRecords.AllAsync(x => x.Status == "Scheduled" && x.UpdatedAtUtc == null)), "migration does not infer historical attendance");
    var afterClass = new AttendanceTestClock(new DateTimeOffset(2030, 4, 1, 0, 0, 0, TimeSpan.Zero));
    var beforeClass = new AttendanceTestClock(new DateTimeOffset(2029, 12, 1, 0, 0, 0, TimeSpan.Zero));
    var roster = (await Read(db => new AttendanceService(db, afterClass).GetRosterAsync(currentSession.Id)))!;
    Assert(roster.Participants.Count == 1 && roster.Participants[0].StudentName == "學生甲", "session roster shows approved student only");
    Assert((await Read(db => new AttendanceService(db, afterClass).GetRosterAsync(-1))) is null, "unknown session returns no roster");
    var participant = roster.Participants.Single();
    var originalVersion = participant.RowVersion;
    AttendanceEditViewModel Mark(string status, byte[]? version = null, string? reason = null, int? targetSession = null) => new()
    {
        SessionId = targetSession ?? currentSession.Id, ParticipantId = participant.ParticipantId,
        RowVersion = version ?? originalVersion, Status = status, ManualReason = reason
    };
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Present"), 0)), "管理員", "non-admin cannot mark attendance");
    await Reject(() => InDb(db => new AttendanceService(db, beforeClass).RecordAsync(Mark("Present"), adminId)), "尚未開始", "future class cannot be marked");
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Invalid"), adminId)), "有效", "unknown attendance status rejected");
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Present", targetSession: -1), adminId)), "名單", "participant cannot be submitted for another session");
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Present", reason: new string('x', 501)), adminId)), "500", "overlong note rejected");
    await InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Present"), adminId));
    Assert(await Read(db => db.AttendanceRecords.AnyAsync(x => x.SessionParticipantId == participant.ParticipantId
        && x.Status == "Present" && x.CheckedInByUserId == adminId && x.CheckInMethod == "Manual" && x.UpdatedAtUtc == afterClass.GetUtcNow().UtcDateTime)), "present persists with actor and timestamp");
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Absent", reason: "過期表單"), adminId)), "已被其他人修改", "stale form cannot overwrite attendance");
    var presentVersion = await Read(db => db.AttendanceRecords.Where(x => x.SessionParticipantId == participant.ParticipantId).Select(x => x.RowVersion).SingleAsync());
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Late", presentVersion), adminId)), "原因", "correction requires a reason");
    foreach (var status in new[] { "Late", "Absent", "Scheduled" })
    {
        var version = await Read(db => db.AttendanceRecords.Where(x => x.SessionParticipantId == participant.ParticipantId).Select(x => x.RowVersion).SingleAsync());
        await InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark(status, version, "核對後更正"), adminId));
        Assert(await Read(db => db.AttendanceRecords.AnyAsync(x => x.SessionParticipantId == participant.ParticipantId && x.Status == status)), "correction persists " + status);
    }
    Assert(await Read(db => db.AttendanceRecords.CountAsync()) == 10, "edits do not duplicate attendance records");
    await InDb(async db =>
    {
        var enrollment = await db.CourseEnrollments.SingleAsync(x => x.EnrollmentApplicationId == applicationId);
        enrollment.Status = "Cancelled";
        await db.SaveChangesAsync();
    });
    await Reject(() => InDb(db => new AttendanceService(db, afterClass).RecordAsync(Mark("Present"), adminId)), "有效名單", "cancelled enrollment cannot be marked");
    Assert(!(await Read(db => new AttendanceService(db, afterClass).GetRosterAsync(currentSession.Id)))!.Participants.Single().IsActive, "cancelled enrollment remains visible as inactive");
    await LeaveMakeupChecks.RunAsync(Db, adminId, studentId, courseId, emptyCourseId, Assert);
    await TrialLessonChecks.RunAsync(Db, adminId, studentId, Assert);
    if (args.Contains("--http")) await HttpWorkflowChecks.RunAsync(Db, adminId, studentId, emptyCourseId, Assert);
    Console.WriteLine($"All {checks} workflow checks passed.");
}
finally
{
    if (created)
    {
        await using var cleanup = Db();
        await cleanup.Database.EnsureDeletedAsync();
        Console.WriteLine("Removed isolated test database: " + database);
    }
}

sealed class AttendanceTestClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
