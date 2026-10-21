using System.Net;
using System.Text.RegularExpressions;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal static class TeacherRequestHttpChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, Uri address, HttpClient admin, Action<bool, string> assert)
    {
        const string password = "Teacher-request-test_2032!";
        long teacherId, courseId, locationId; int sessionId;
        await using (var db = createDb())
        {
            var teacher = await db.Teachers.Include(x => x.User).SingleAsync(x => x.User.Account == "teacher-requests");
            teacher.User.PasswordHash = new PasswordHasher<User>().HashPassword(teacher.User, password);
            teacher.User.IsActive = false;
            await db.SaveChangesAsync(); teacherId = teacher.TeacherId;
            var session = await db.ClassSessions.Where(x => x.TeacherId == teacherId && !x.IsExtraSession).OrderBy(x => x.StartsAt).FirstAsync();
            courseId = session.CourseId; locationId = session.StudioLocationId; sessionId = session.Id;
        }
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
        using var teacherClient = new HttpClient(handler) { BaseAddress = address };
        async Task<string> Page(HttpClient client, string path)
        {
            using var response = await client.GetAsync(path); response.EnsureSuccessStatusCode();
            return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        }
        static string Hidden(string html, string name) => Regex.Match(html, "name=\"" + Regex.Escape(name) + "\"[^>]*value=\"([^\"]*)\"").Groups[1].Value;
        async Task Post(HttpClient client, string path, Dictionary<string, string> fields, HttpStatusCode expected)
        {
            using var response = await client.PostAsync(path, new FormUrlEncodedContent(fields));
            if (response.StatusCode != expected) throw new Exception($"{path}: expected {expected}, got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
        using (var anon = await teacherClient.GetAsync("/TeacherRequests")) assert(anon.StatusCode == HttpStatusCode.Redirect, "HTTP anonymous cannot access teacher requests");
        var edit = await Page(admin, "/Admin/Teachers/Edit/" + teacherId);
        assert(edit.Contains("teacher-requests") && edit.Contains("啟用登入帳號"), "HTTP teacher edit exposes account and activation control");
        var activation = new Dictionary<string, string> { ["TeacherId"] = teacherId.ToString(), ["DisplayName"] = "申請測試老師",
            ["RowVersion"] = Hidden(edit, "RowVersion"), ["IsActive"] = "true", ["IsUserActive"] = "true",
            ["__RequestVerificationToken"] = Hidden(edit, "__RequestVerificationToken") };
        await Post(admin, "/Admin/Teachers/Edit/" + teacherId, activation, HttpStatusCode.OK);
        await using (var db = createDb()) assert(!await db.Teachers.AnyAsync(x => x.TeacherId == teacherId && x.User.IsActive), "HTTP activation requires teacher email");
        activation["Email"] = "teacher-requests@example.test";
        await Post(admin, "/Admin/Teachers/Edit/" + teacherId, activation, HttpStatusCode.Redirect);
        await using (var db = createDb()) assert(await db.Teachers.AnyAsync(x => x.TeacherId == teacherId && x.User.IsActive), "HTTP admin can enable teacher account");
        var login = await Page(teacherClient, "/Account/Login");
        using (var signedIn = await teacherClient.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["Account"] = "teacher-requests", ["Password"] = password, ["ReturnUrl"] = "/Admin",
            ["__RequestVerificationToken"] = Hidden(login, "__RequestVerificationToken") })))
            assert(signedIn.StatusCode == HttpStatusCode.Redirect && signedIn.Headers.Location?.ToString() == "/TeacherRequests",
                "HTTP teacher login redirects to teacher workspace even from Admin entry");
        assert((await Page(teacherClient, "/TeacherRequests")).Contains("申請加開課"), "HTTP teacher can login and see own requests");
        using (var denied = await teacherClient.GetAsync("/Admin/Payments")) assert(denied.StatusCode == HttpStatusCode.Redirect, "HTTP teacher cannot access admin payments");
        var form = await Page(teacherClient, "/TeacherRequests/Create");
        var fields = new Dictionary<string, string> { ["SubmissionKey"] = Hidden(form, "SubmissionKey"), ["Kind"] = "ExtraSession", ["TeacherId"] = teacherId.ToString(),
            ["CourseId"] = courseId.ToString(), ["StudioLocationId"] = locationId.ToString(), ["StartsAtTaiwan"] = "2032-02-01T10:00", ["EndsAtTaiwan"] = "2032-02-01T11:00",
            ["RequestedCapacity"] = "6", ["Reason"] = "HTTP 加開課" };
        await Post(teacherClient, "/TeacherRequests/Create", fields, HttpStatusCode.BadRequest);
        assert(true, "HTTP teacher submission requires antiforgery token");
        fields["__RequestVerificationToken"] = Hidden(form, "__RequestVerificationToken");
        await Post(teacherClient, "/TeacherRequests/Create", fields, HttpStatusCode.Redirect);
        await Post(teacherClient, "/TeacherRequests/Create", fields, HttpStatusCode.Redirect);
        long id;
        await using (var db = createDb()) id = await db.TeacherRequests.Where(x => x.SubmissionKey == Guid.Parse(fields["SubmissionKey"])).Select(x => x.Id).SingleAsync();
        assert(true, "HTTP teacher form replay creates only one request");
        var detail = await Page(teacherClient, "/TeacherRequests/Details/" + id);
        assert(detail.Contains("HTTP 加開課") && !detail.Contains("name=\"Decision\""), "HTTP teacher sees request without admin review form");
        await Post(teacherClient, "/TeacherRequests/Review", new() { ["Id"] = id.ToString(), ["Decision"] = "Approve", ["ConfirmDetails"] = "true",
            ["__RequestVerificationToken"] = Hidden(detail, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
        await using (var db = createDb()) assert(await db.TeacherRequests.AnyAsync(x => x.Id == id && x.Status == "Pending"), "HTTP teacher forged review cannot approve request");
        detail = await Page(admin, "/TeacherRequests/Details/" + id);
        var review = new Dictionary<string, string> { ["Id"] = id.ToString(), ["RowVersion"] = Hidden(detail, "RowVersion"), ["Decision"] = "Approve", ["ConfirmDetails"] = "true",
            ["__RequestVerificationToken"] = Hidden(detail, "__RequestVerificationToken") };
        await Post(admin, "/TeacherRequests/Review", review, HttpStatusCode.Redirect);
        await using (var db = createDb()) assert(await db.TeacherRequests.AnyAsync(x => x.Id == id && x.Status == "Approved" && x.CreatedSession!.IsExtraSession), "HTTP admin approval creates extra session");
        var capForm = await Page(admin, "/TeacherRequests/Create?kind=Capacity&teacherId=" + teacherId);
        await Post(admin, "/TeacherRequests/Create", new() { ["SubmissionKey"] = Hidden(capForm, "SubmissionKey"), ["Kind"] = "Capacity", ["TeacherId"] = teacherId.ToString(),
            ["TargetSessionId"] = sessionId.ToString(), ["RequestedCapacity"] = "15", ["Reason"] = "HTTP 超收", ["__RequestVerificationToken"] = Hidden(capForm, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
        await using (var db = createDb()) id = await db.TeacherRequests.Where(x => x.Reason == "HTTP 超收").Select(x => x.Id).SingleAsync();
        detail = await Page(admin, "/TeacherRequests/Details/" + id);
        review["Id"] = id.ToString(); review["RowVersion"] = Hidden(detail, "RowVersion"); review["__RequestVerificationToken"] = Hidden(detail, "__RequestVerificationToken");
        await Post(admin, "/TeacherRequests/Review", review, HttpStatusCode.Redirect);
        assert((await Page(admin, "/Admin/ClassSessions/Details/" + sessionId)).Contains("15"), "HTTP admin proxy request and capacity approval update session detail");
        await using (var db = createDb()) { var t = await db.Teachers.FindAsync(teacherId); t!.IsActive = false; await db.SaveChangesAsync(); }
        using (var disabled = await teacherClient.GetAsync("/TeacherRequests")) assert(disabled.StatusCode == HttpStatusCode.Redirect, "HTTP disabled teacher loses access with existing cookie");
        await using (var db = createDb())
        {
            var teacher = await db.Teachers.Include(x => x.User).SingleAsync(x => x.TeacherId == teacherId);
            teacher.IsActive = true;
            teacher.User.PasswordHash = new PasswordHasher<User>().HashPassword(teacher.User, "Updated-password_2032!");
            await db.SaveChangesAsync();
        }
        using (var expired = await teacherClient.GetAsync("/TeacherRequests")) assert(expired.StatusCode == HttpStatusCode.Redirect,
            "HTTP password change revokes old teacher login cookie");
    }
}
