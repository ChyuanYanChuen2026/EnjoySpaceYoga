using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

internal static class HttpWorkflowChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, long adminId, long studentId, long targetCourse, Action<bool, string> assert)
    {
        const string password = "Isolated-test-only_2030!";
        string connection;
        long participantId;
        int targetId;
        await using (var db = createDb())
        {
            connection = db.Database.GetConnectionString()!;
            var admin = await db.Users.FindAsync(adminId);
            admin!.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);
            await db.SaveChangesAsync();
            participantId = await db.ClassSessionParticipants.Where(x => x.StudentId == studentId && x.Status == "Active" && x.ParticipationType == "Regular")
                .OrderBy(x => x.ClassSessionId).Select(x => x.SessionParticipantId).FirstAsync();
            targetId = await db.ClassSessions.Where(x => x.CourseId == targetCourse).Select(x => x.Id).SingleAsync();
        }
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "EnjoySpace.Web.csproj"))) root = root.Parent;
        if (root is null) throw new InvalidOperationException("Cannot locate MVC content root for HTTP checks.");
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = root.FullName };
        start.ArgumentList.Add(typeof(AppDbContext).Assembly.Location);
        start.ArgumentList.Add("--urls"); start.ArgumentList.Add("http://127.0.0.1:0");
        start.ArgumentList.Add("--environment"); start.ArgumentList.Add("Development");
        start.Environment["ConnectionStrings__DefaultConnection"] = connection;
        start.Environment["Seed__SyncAutumn2026Schedule"] = "false";
        var address = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var process = new Process { StartInfo = start };
        process.OutputDataReceived += (_, line) =>
        {
            if (line.Data is { } data && data.Contains("Now listening on: "))
                address.TrySetResult(data[(data.IndexOf("Now listening on: ", StringComparison.Ordinal) + 18)..].Trim());
        };
        process.ErrorDataReceived += (_, _) => { };
        process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
        try
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(await address.Task.WaitAsync(TimeSpan.FromSeconds(20))) };
            async Task<string> Page(string path)
            {
                using var response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();
                return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
            }
            string Hidden(string html, string name)
            {
                var match = Regex.Match(html, "name=\"" + Regex.Escape(name) + "\"[^>]*value=\"([^\"]*)\"");
                if (!match.Success) throw new Exception("Missing form field: " + name);
                return match.Groups[1].Value;
            }
            async Task Post(string path, Dictionary<string, string> values, HttpStatusCode expected)
            {
                using var response = await client.PostAsync(path, new FormUrlEncodedContent(values));
                if (response.StatusCode != expected) throw new Exception($"HTTP {path}: expected {expected}, got {response.StatusCode}");
            }
            using (var anonymous = await client.GetAsync("/Admin/LeaveMakeup"))
                assert(anonymous.StatusCode == HttpStatusCode.Redirect && anonymous.Headers.Location!.ToString().Contains("/Account/Login"), "HTTP leave management requires login");
            var login = await Page("/Account/Login");
            await Post("/Account/Login", new() { ["Account"] = "admin-check", ["Password"] = password,
                ["__RequestVerificationToken"] = Hidden(login, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
            foreach (var tab in new[] { "leaves", "entitlements", "requests" })
                assert((await Page("/Admin/LeaveMakeup?tab=" + tab)).Contains("請假與補課"), "HTTP renders management tab " + tab);
            var leavePage = await Page("/Admin/LeaveMakeup/Leave?participantId=" + participantId);
            var leaveFields = new Dictionary<string, string> { ["ParticipantId"] = participantId.ToString(),
                ["RowVersion"] = Hidden(leavePage, "RowVersion"), ["Reason"] = "HTTP 表單請假" };
            await Post("/Admin/LeaveMakeup/Leave", leaveFields, HttpStatusCode.BadRequest);
            assert(true, "HTTP leave POST rejects missing antiforgery token");
            leaveFields["__RequestVerificationToken"] = Hidden(leavePage, "__RequestVerificationToken");
            var invalid = new Dictionary<string, string>(leaveFields) { ["RowVersion"] = "not-base64" };
            await Post("/Admin/LeaveMakeup/Leave", invalid, HttpStatusCode.OK);
            assert(true, "HTTP invalid rowversion redisplays validation without crashing");
            await Post("/Admin/LeaveMakeup/Leave", leaveFields, HttpStatusCode.Redirect);
            long leaveId, entitlementId;
            await using (var db = createDb())
            {
                var leave = await db.LeaveRequests.Include(x => x.Entitlement).SingleAsync(x => x.SessionParticipantId == participantId && x.Status == "Active");
                leaveId = leave.LeaveRequestId; entitlementId = leave.Entitlement.MakeupEntitlementId;
            }
            assert(true, "HTTP leave creates qualification");
            var apply = await Page("/Admin/LeaveMakeup/Apply?entitlementId=" + entitlementId);
            await Post("/Admin/LeaveMakeup/Apply", new() { ["EntitlementId"] = entitlementId.ToString(), ["RowVersion"] = Hidden(apply, "RowVersion"),
                ["TargetSessionId"] = targetId.ToString(), ["__RequestVerificationToken"] = Hidden(apply, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
            long requestId; byte[] version;
            await using (var db = createDb())
            {
                var request = await db.MakeupRequests.SingleAsync(x => x.MakeupEntitlementId == entitlementId);
                requestId = request.MakeupRequestId; version = request.RowVersion;
            }
            var requestsPage = await Page("/Admin/LeaveMakeup?tab=requests");
            var action = new Dictionary<string, string> { ["Id"] = requestId.ToString(), ["RowVersion"] = Convert.ToBase64String(version), ["Approve"] = "true",
                ["__RequestVerificationToken"] = Hidden(requestsPage, "__RequestVerificationToken") };
            await Post("/Admin/LeaveMakeup/Review", action, HttpStatusCode.Redirect);
            await using (var db = createDb())
            {
                var request = await db.MakeupRequests.SingleAsync(x => x.MakeupRequestId == requestId);
                assert(request.Status == "Approved", "HTTP approval changes request state");
                version = request.RowVersion;
            }
            var roster = await Page("/Admin/Attendance?sessionId=" + targetId);
            assert(roster.Contains("補課") && roster.Contains("學生甲"), "HTTP attendance renders approved makeup participant");
            action["RowVersion"] = Convert.ToBase64String(version); action["Note"] = "HTTP 取消補課";
            await Post("/Admin/LeaveMakeup/CancelMakeup", action, HttpStatusCode.Redirect);
            await using (var db = createDb()) version = (await db.LeaveRequests.FindAsync(leaveId))!.RowVersion;
            action["Id"] = leaveId.ToString(); action["RowVersion"] = Convert.ToBase64String(version);
            await Post("/Admin/LeaveMakeup/CancelLeave", action, HttpStatusCode.Redirect);
            await using (var db = createDb())
                assert(await db.LeaveRequests.AnyAsync(x => x.LeaveRequestId == leaveId && x.Status == "Cancelled" && x.Entitlement.Status == "Revoked"), "HTTP cancellation round trip restores original enrollment");

            var guestPage = await Page("/Admin/TrialGuests/Edit");
            await Post("/Admin/TrialGuests/Edit", new() { ["Id"] = "0", ["RowVersion"] = "", ["Name"] = "HTTP體驗生", ["Phone"] = "0900111222", ["SourceType"] = "Direct", ["IsActive"] = "true",
                ["__RequestVerificationToken"] = Hidden(guestPage, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
            long guestId, planId;
            await using (var db = createDb()) guestId = await db.TrialGuests.Where(x => x.Name == "HTTP體驗生").Select(x => x.TrialGuestId).SingleAsync();
            assert((await Page("/Admin/TrialGuests")).Contains("HTTP體驗生"), "HTTP guest form creates and lists trial guest");
            var planPage = await Page("/Admin/LessonPlans/Edit");
            await Post("/Admin/LessonPlans/Edit", new() { ["Id"] = "0", ["RowVersion"] = "", ["PlanCode"] = "HTTP-TRIAL", ["PlanName"] = "HTTP體驗方案", ["PlanType"] = "Trial",
                ["SessionQuota"] = "1", ["Price"] = "250", ["ValidityUnit"] = "Days", ["ValidityValue"] = "30", ["IsActive"] = "true",
                ["__RequestVerificationToken"] = Hidden(planPage, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
            await using (var db = createDb()) planId = await db.LessonPlans.Where(x => x.PlanCode == "HTTP-TRIAL").Select(x => x.LessonPlanId).SingleAsync();
            assert((await Page("/Admin/LessonPlans")).Contains("HTTP體驗方案"), "HTTP plan form creates and lists plan");
            var buyPage = await Page("/Admin/LessonPasses/Create?ownerType=TrialGuest&ownerId=" + guestId);
            var purchaseValues = new Dictionary<string, string> { ["PlanId"] = planId.ToString(), ["OwnerType"] = "TrialGuest", ["OwnerId"] = guestId.ToString(),
                ["ValidFromDate"] = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd"), ["__RequestVerificationToken"] = Hidden(buyPage, "__RequestVerificationToken") };
            string quote;
            using (var response = await client.PostAsync("/Admin/LessonPasses/Preview", new FormUrlEncodedContent(purchaseValues)))
            { response.EnsureSuccessStatusCode(); quote = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()); }
            assert(quote.Contains("HTTP體驗方案") && quote.Contains("250.00"), "HTTP purchase preview renders server quote");
            var confirmValues = new Dictionary<string, string> { ["token"] = Hidden(quote, "token"), ["confirmDetails"] = "true", ["__RequestVerificationToken"] = Hidden(quote, "__RequestVerificationToken") };
            await Post("/Admin/LessonPasses/Purchase", confirmValues, HttpStatusCode.Redirect);
            await Post("/Admin/LessonPasses/Purchase", confirmValues, HttpStatusCode.Redirect);
            long passId;
            await using (var db = createDb()) passId = await db.LessonPasses.Where(x => x.TrialGuestId == guestId).Select(x => x.LessonPassId).SingleAsync();
            assert(true, "HTTP repeated purchase confirmation creates one order");
            var detail = await Page("/Admin/LessonPasses/Details/" + passId);
            await Post("/Admin/LessonPasses/ConfirmPayment", new() { ["Id"] = passId.ToString(), ["RowVersion"] = Hidden(detail, "RowVersion"), ["ReceivedAmount"] = "250",
                ["Method"] = "Cash", ["PaidAtTaiwan"] = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-ddTHH:mm"), ["ConfirmDetails"] = "true",
                ["__RequestVerificationToken"] = Hidden(detail, "__RequestVerificationToken") }, HttpStatusCode.Redirect);
            await using (var db = createDb()) assert(await db.LessonPasses.AnyAsync(x => x.LessonPassId == passId && x.Status == "Active" && x.Payment.Status == "Confirmed"), "HTTP receipt activates trial pass");
            assert((await Page("/Admin/LessonPasses")).Contains("HTTP體驗方案"), "HTTP purchase history renders order");
            var paymentPage = await Page("/Admin/Payments");
            assert(paymentPage.Contains("HTTP體驗方案") && paymentPage.Contains("HTTP體驗生"), "HTTP unified payment list includes trial payment");
            await PortalHttpChecks.RunAsync(createDb, client.BaseAddress!, client, targetId, assert);
        }
        finally
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
    }
}
