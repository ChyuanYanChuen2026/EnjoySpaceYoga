using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

public abstract class TrialLessonControllerBase : Controller
{
    protected long ActorId => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    protected async Task<bool> Attempt(Func<Task> action)
    {
        try { await action(); TempData.Remove("Error"); return true; }
        catch (InvalidOperationException e) { ModelState.AddModelError("", e.Message); TempData["Error"] = e.Message; }
        catch (DbUpdateConcurrencyException) { Error("資料已被其他人修改，請重新載入並核對。"); }
        catch (DbUpdateException) { Error("資料已變更或代碼重複，請重新核對後操作。"); }
        catch (SqlException e) when (e.Number == 1205) { Error("其他人正在更新資料，請重新載入後再試。"); }
        return false;
    }
    private void Error(string message) { ModelState.AddModelError("", message); TempData["Error"] = message; }
}
