using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace EnjoySpace.Web.Controllers;

[ApiController, Route("api/portal/check-in"), AutoValidateAntiforgeryToken]
[Authorize(AuthenticationSchemes = PortalAuthentication.Scheme, Roles = "Student")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[EnableRateLimiting("check-in")]
public class PortalCheckInController(CheckInService service, ILogger<PortalCheckInController> logger) : ControllerBase
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult> Run(Func<Task<object>> work)
    {
        try { return Ok(await work()); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (CheckInFault e) { return Conflict(new { code = e.Code, message = e.Message, sessionId = e.SessionId }); }
        catch (Exception e) {
            logger.LogError(e, "Unexpected check-in error");
            await service.LogErrorAsync(Actor, null, "UNKNOWN_ERROR", "打卡發生不明錯誤，請查詢結果或洽管理員。");
            return StatusCode(503, new { code = "UNKNOWN_ERROR", message = "暫時無法確認打卡結果，請查詢結果或洽管理員。" });
        }
    }
    [HttpPost("preview")] public Task<IActionResult> Preview(CheckInInput input) => Run(() => service.ScanAsync(input.Token, Actor, false));
    [HttpPost] public Task<IActionResult> Confirm(CheckInInput input) => Run(() => service.ScanAsync(input.Token, Actor, true));
    [HttpGet("sessions/{id:int}/receipt")] public Task<IActionResult> Receipt(int id) => Run(() => service.ReceiptAsync(id, Actor));
    [HttpPost("sessions/{id:int}/report")] public Task<IActionResult> Report(int id) => Run(async () => {
        await service.ReceiptAsync(id, Actor); // Verify ownership before accepting a client report.
        await service.LogErrorAsync(Actor, id, "CLIENT_UNCERTAIN", "學生端曾中斷連線，請核對實際出勤；此回報不更改出勤。");
        return new { message = "已通知工作室核對打卡紀錄。" };
    });
}
public class CheckInInput { [Required, StringLength(256)] public string Token { get; set; } = ""; }
