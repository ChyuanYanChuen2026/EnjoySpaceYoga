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
        var credentials = new { account = "portal-guest", password = "Portal-test-2030!", confirmPassword = "Portal-test-2030!", name = "官網體驗生", email = "portal@example.test", phone = "0912345678" };
        assert((await Post("register", credentials, false)).StatusCode == HttpStatusCode.BadRequest, "portal registration requires antiforgery token");
        assert((await Post("register", credentials)).IsSuccessStatusCode, "portal creates trial account in shared Users");
        assert((await Post("login", credentials)).IsSuccessStatusCode, "portal trial account login succeeds");
        assert((await client.GetAsync("/api/portal/me")).IsSuccessStatusCode, "portal authenticated cookie validated against shared roles");
        assert((await client.GetAsync("/Admin/PortalTrials")).StatusCode == HttpStatusCode.Redirect, "portal cookie cannot enter MVC admin");
        var key = Guid.NewGuid();
        var input = new { submissionKey = key, sessionId, planId, name = "官網體驗生", email = "portal@example.test", phone = "0912345678", note = "API 整合驗證" };
        var applied = await Post("trial-requests", input);
        if (!applied.IsSuccessStatusCode) throw new Exception("Portal apply failed: " + await applied.Content.ReadAsStringAsync());
        assert(true, "portal trial request submitted");
        assert((await Post("trial-requests", input)).IsSuccessStatusCode, "portal repeated submission is idempotent");
        await using (var db = createDb())
        {
            var request = await db.PortalTrialRequests.Include(x => x.TrialGuest).SingleAsync(x => x.SubmissionKey == key);
            assert(request.QuotedPrice == 321 && request.TrialGuest.Name == "官網體驗生", "portal request uses server price and shared TrialGuests");
            assert(await db.PortalTrialRequests.CountAsync(x => x.SubmissionKey == key) == 1, "portal replay creates exactly one request");
        }
        var dashboard = await client.GetFromJsonAsync<JsonElement>("/api/portal/dashboard");
        assert(dashboard.GetProperty("requests").GetArrayLength() == 1, "portal dashboard returns own request");
        assert(WebUtility.HtmlDecode(await admin.GetStringAsync("/Admin/PortalTrials")).Contains("官網體驗生"), "MVC admin displays portal submission");
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.Account == "portal-guest"); user.IsActive = false; await db.SaveChangesAsync();
        }
        assert((await client.GetAsync("/api/portal/me")).StatusCode == HttpStatusCode.Unauthorized, "deactivating user immediately revokes portal access");
    }
}
