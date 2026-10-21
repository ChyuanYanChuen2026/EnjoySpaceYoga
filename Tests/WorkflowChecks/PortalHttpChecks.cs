using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

internal static class PortalHttpChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, Uri address, HttpClient admin, int sessionId, Action<bool, string> assert)
    {
        long planId;
        await using (var db = createDb())
        {
            if (!await db.Roles.AnyAsync(x => x.RoleCode == "Student"))
                db.Roles.Add(new Role { RoleCode = "Student", RoleName = "學生" });
            db.Roles.Add(new Role { RoleCode = "TrialGuest", RoleName = "體驗生" });
            var plan = new LessonPlan { PlanCode = "PORTAL", PlanName = "官網體驗", PlanType = "Trial", AudienceType = "TrialGuest", Price = 321, SessionQuota = 1 };
            db.LessonPlans.Add(plan);
            await db.SaveChangesAsync(); planId = plan.LessonPlanId;
        }
        using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
        using var client = new HttpClient(handler) { BaseAddress = address };
        async Task<string> Token() => (await client.GetFromJsonAsync<JsonElement>("/api/portal/csrf")).GetProperty("token").GetString()!;
        async Task<HttpResponseMessage> Post(string path, object body, bool csrf = true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/portal/" + path) { Content = JsonContent.Create(body) };
            if (csrf) request.Headers.Add("X-CSRF-TOKEN", await Token());
            return await client.SendAsync(request);
        }
        assert((await client.GetAsync("/api/portal/me")).StatusCode == HttpStatusCode.Unauthorized, "portal anonymous API returns 401 without HTML redirect");
        assert((await admin.GetAsync("/api/portal/me")).StatusCode == HttpStatusCode.Unauthorized, "MVC admin cookie does not authenticate portal");
        assert((await client.GetAsync("/api/portal/catalog")).IsSuccessStatusCode, "portal public catalog SQL query succeeds");
        assert((await client.GetAsync("/api/portal/sessions?from=2030-01-01&until=2030-03-01")).IsSuccessStatusCode, "portal public sessions SQL query succeeds");
        assert((await client.GetAsync("/api/portal/sessions?from=2030-01-01&until=2031-01-01")).StatusCode == HttpStatusCode.BadRequest, "portal rejects unbounded date ranges");
        var selectedSessions = await client.GetFromJsonAsync<JsonElement>("/api/portal/sessions?sessionId=" + sessionId);
        assert(selectedSessions.GetArrayLength() == 1 && selectedSessions[0].GetProperty("id").GetInt32() == sessionId,
            "portal selected session can be loaded beyond default sixty day window");
        await using (var db = createDb())
        {
            var courseId = await db.ClassSessions.Where(x => x.Id == sessionId).Select(x => x.CourseId).SingleAsync();
            await db.Courses.Where(x => x.CourseId == courseId).ExecuteUpdateAsync(x => x.SetProperty(c => c.IsPubliclyVisible, false));
            var hidden = await client.GetFromJsonAsync<JsonElement>("/api/portal/sessions?sessionId=" + sessionId);
            assert(hidden.GetArrayLength() == 0, "portal selected session does not bypass course visibility");
            await db.Courses.Where(x => x.CourseId == courseId).ExecuteUpdateAsync(x => x.SetProperty(c => c.IsPubliclyVisible, true));
        }
        var credentials = new { account = "portal-guest", password = "Portal-test-2030!", confirmPassword = "Portal-test-2030!", name = "官網體驗生", email = "portal@example.test", phone = "0912345678" };
        assert((await Post("register", credentials, false)).StatusCode == HttpStatusCode.BadRequest, "portal registration requires antiforgery token");
        long guestCount;
        await using (var db = createDb()) guestCount = await db.TrialGuests.CountAsync();
        assert((await Post("register", credentials)).IsSuccessStatusCode, "portal creates student account in shared Users");
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.Account == "portal-guest");
            var roles = await db.UserRoles.Where(x => x.UserId == user.UserId).Select(x => x.Role.RoleCode).ToListAsync();
            assert(roles.SequenceEqual(new[] { "Student" }), "registration assigns Student only, not TrialGuest or privileged roles");
            assert(await db.Students.CountAsync(x => x.UserId == user.UserId && x.IsActive) == 1, "registration creates one active student profile atomically");
            assert(await db.TrialGuests.CountAsync() == guestCount, "registration does not create a trial guest before application");
        }
        assert((await Post("login", credentials)).IsSuccessStatusCode, "new student account can login without admin repair");
        assert((await client.GetAsync("/api/portal/me")).IsSuccessStatusCode, "portal authenticated cookie validated against shared roles");
        assert((await client.GetAsync("/Admin/PortalTrials")).StatusCode == HttpStatusCode.Redirect, "portal cookie cannot enter MVC admin");
        var key = Guid.NewGuid();
        var input = new { submissionKey = key, sessionId, planId, name = "官網體驗生", email = "portal@example.test", phone = "0912345678", note = "API 整合驗證" };
        assert((await Post("trial-requests", input, false)).StatusCode == HttpStatusCode.BadRequest, "trial application still requires CSRF protection");
        assert((await Post("trial-requests", new { submissionKey = key, sessionId, planId = 0, name = "官網體驗生", email = "portal@example.test", phone = "0912345678" })).StatusCode == HttpStatusCode.BadRequest,
            "trial application requires a selected plan");
        await using (var db = createDb())
        {
            assert(await db.TrialGuests.CountAsync() == guestCount, "invalid application cannot create a trial guest");
            await db.LessonPlans.Where(x => x.LessonPlanId == planId).ExecuteUpdateAsync(x => x.SetProperty(p => p.IsActive, false));
        }
        assert((await Post("trial-requests", input)).StatusCode == HttpStatusCode.BadRequest, "trial API rejects a plan disabled after selection");
        await using (var db = createDb())
            await db.LessonPlans.Where(x => x.LessonPlanId == planId).ExecuteUpdateAsync(x => x.SetProperty(p => p.IsActive, true));
        var applied = await Post("trial-requests", input);
        if (!applied.IsSuccessStatusCode) throw new Exception("Portal apply failed: " + await applied.Content.ReadAsStringAsync());
        assert(true, "portal trial request submitted");
        assert((await Post("trial-requests", input)).IsSuccessStatusCode, "portal repeated submission is idempotent");
        await using (var db = createDb())
        {
            var request = await db.PortalTrialRequests.Include(x => x.TrialGuest).SingleAsync(x => x.SubmissionKey == key);
            assert(request.QuotedPrice == 321 && request.TrialGuest.Name == "官網體驗生", "portal request uses server price and shared TrialGuests");
            assert(await db.PortalTrialRequests.CountAsync(x => x.SubmissionKey == key) == 1, "portal replay creates exactly one request");
            assert(await db.TrialGuests.CountAsync() == guestCount + 1, "successful application creates exactly one trial guest, including replay");
            assert(await db.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.Role.RoleCode == "Student")
                && !await db.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.Role.RoleCode == "TrialGuest"),
                "trial guest record does not replace the applicant student account role");
        }
        var dashboard = await client.GetFromJsonAsync<JsonElement>("/api/portal/dashboard");
        assert(dashboard.GetProperty("requests").GetArrayLength() == 1, "portal dashboard returns own request");
        assert(WebUtility.HtmlDecode(await admin.GetStringAsync("/Admin/PortalTrials")).Contains("官網體驗生"), "MVC admin displays portal submission");
        await using (var db = createDb())
            await db.PortalTrialRequests.Where(x => x.SubmissionKey == key).ExecuteUpdateAsync(x => x.SetProperty(r => r.Status, "Contacted"));
        assert((await Post("trial-requests", new { submissionKey = Guid.NewGuid(), sessionId, planId, name = "官網體驗生", email = "portal@example.test", phone = "0912345678" })).StatusCode == HttpStatusCode.Conflict,
            "portal contacted request cannot be submitted again for same session");
        long referralPlanId;
        int referralSessionId;
        int seatsBefore, paymentsBefore;
        await using (var db = createDb())
        {
            var plan = new LessonPlan { PlanCode = "PORTAL-REFERRAL", PlanName = "舊生介紹一堂免費", Price = 0, SessionQuota = 1, RequiresReferrer = true, ValidityUnit = "Unlimited", ValidityValue = null };
            db.LessonPlans.Add(plan); await db.SaveChangesAsync(); referralPlanId = plan.LessonPlanId;
            var publicCatalog = new EnjoySpace.Web.Services.PortalCatalogService(db);
            referralSessionId = await publicCatalog.Sessions
                .Where(x => x.Id != sessionId && x.StartsAt > DateTimeOffset.UtcNow && publicCatalog.Participants.Count(p => p.ClassSessionId == x.Id) < (x.CapacityOverride ?? x.Capacity))
                .Select(x => x.Id).FirstAsync();
            seatsBefore = await db.ClassSessionParticipants.CountAsync(); paymentsBefore = await db.Payments.CountAsync();
        }
        var referralKey = Guid.NewGuid();
        object Referral(string? name) => new { submissionKey = referralKey, sessionId = referralSessionId, planId = referralPlanId, name = "官網體驗生", email = "portal@example.test", phone = "0912345678", referrerName = name };
        assert((await Post("trial-requests", Referral(null))).StatusCode == HttpStatusCode.BadRequest, "referral plan cannot bypass required referrer through API");
        assert((await Post("trial-requests", Referral("   "))).StatusCode == HttpStatusCode.BadRequest, "referral API rejects whitespace referrer");
        var referralApplied = await Post("trial-requests", Referral("介紹人測試"));
        if (!referralApplied.IsSuccessStatusCode) throw new Exception($"Referral apply failed ({referralApplied.StatusCode}): " + await referralApplied.Content.ReadAsStringAsync());
        assert(true, "student submits free referral application for manual review");
        assert((await Post("trial-requests", Referral("介紹人測試"))).IsSuccessStatusCode, "referral replay is idempotent");
        await using (var db = createDb())
        {
            var request = await db.PortalTrialRequests.SingleAsync(x => x.SubmissionKey == referralKey);
            assert(request.ReferrerName == "介紹人測試" && request.QuotedPrice == 0 && request.Status == "Pending", "referrer and free quote saved as pending application");
            assert(await db.ClassSessionParticipants.CountAsync() == seatsBefore && await db.Payments.CountAsync() == paymentsBefore,
                "trial application cannot automatically enroll or charge before admin review");
            assert(await db.TrialGuests.CountAsync() == guestCount + 1, "same account reuses its own trial guest for referral");
        }
        assert(WebUtility.HtmlDecode(await admin.GetStringAsync("/Admin/PortalTrials")).Contains("介紹人測試"), "admin can see submitted referrer for manual verification");
        await using (var db = createDb())
            await db.Students.Where(x => x.User.Account == "portal-guest").ExecuteUpdateAsync(x => x.SetProperty(s => s.IsActive, false));
        assert((await client.GetAsync("/api/portal/me")).StatusCode == HttpStatusCode.Unauthorized, "deactivating student profile revokes portal session even while User remains active");
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.Account == "portal-guest");
            (await db.Students.SingleAsync(x => x.UserId == user.UserId)).IsActive = true;
            user.IsActive = false; await db.SaveChangesAsync();
        }
        assert((await client.GetAsync("/api/portal/me")).StatusCode == HttpStatusCode.Unauthorized, "deactivating user immediately revokes portal access");
    }
}
