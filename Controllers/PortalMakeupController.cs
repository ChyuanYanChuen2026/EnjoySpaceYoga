using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Controllers;

[ApiController, Route("api/portal/makeup"), AutoValidateAntiforgeryToken]
[Authorize(AuthenticationSchemes = PortalAuthentication.Scheme, Roles = "Student")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PortalMakeupController(AppDbContext db, EnrollmentWorkflowService enrollment, LeaveMakeupService workflow) : ControllerBase
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult> Attempt(Func<long, Task<object>> action)
    {
        try { return Ok(await action(await enrollment.RequireStudentAsync(Actor))); }
        catch (UnauthorizedAccessException) { return NotFound(new { message = "找不到您的補課紀錄，請重新整理。" }); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { message = "補課資料已更新，請先重新整理確認申請結果，再進行操作。" }); }
        catch (SqlException e) when (e.Number == 1205) { return Conflict(new { message = "補課資料正在更新，請先重新整理確認申請結果，再進行操作。" }); }
    }

    [HttpGet]
    public Task<IActionResult> List() => Attempt(studentId => workflow.GetStudentMakeupsAsync(studentId, Actor));

    [HttpGet("entitlements/{id:long}/options")]
    public Task<IActionResult> Options(long id) => Attempt(_ => workflow.GetStudentMakeupOptionsAsync(id, Actor));

    [HttpPost("requests")]
    public Task<IActionResult> Create(PortalMakeupInput input) => Attempt(async studentId =>
    {
        if (!await db.MakeupEntitlements.AnyAsync(x => x.MakeupEntitlementId == input.EntitlementId && x.StudentId == studentId))
            throw new UnauthorizedAccessException();
        var id = await workflow.CreateMakeupAsync(new MakeupCreateModel { EntitlementId = input.EntitlementId,
            TargetSessionId = input.TargetSessionId, RowVersion = input.RowVersion }, Actor, true, input.TargetRowVersion);
        return new { id, message = "補課申請已送出，請等候工作室審核；核准後會加入您的課表。" };
    });

    [HttpPost("requests/{id:long}/cancel")]
    public Task<IActionResult> Cancel(long id, PortalMakeupCancelInput input) => Attempt(async studentId =>
    {
        if (!await db.MakeupRequests.AnyAsync(x => x.MakeupRequestId == id && x.Entitlement.StudentId == studentId))
            throw new UnauthorizedAccessException();
        await workflow.CancelMakeupAsync(new LeaveMakeupActionModel { Id = id, RowVersion = input.RowVersion, Note = input.Reason }, Actor, true);
        return new { message = "補課已取消。您可以在資格有效期間內重新選擇場次；如需取消原課堂請假，請到「我的課程」查看。" };
    });
}

public class PortalMakeupInput : PortalLeaveCancelInput
{
    [Range(1, long.MaxValue)] public long EntitlementId { get; set; }
    [Range(1, int.MaxValue)] public int TargetSessionId { get; set; }
    [Required, MinLength(8), MaxLength(8)] public byte[] TargetRowVersion { get; set; } = [];
}
public class PortalMakeupCancelInput : PortalLeaveCancelInput
{
    [Required(ErrorMessage = "請填寫取消原因。"), StringLength(500, ErrorMessage = "取消原因最多 500 字。")]
    public string Reason { get; set; } = "";
}
