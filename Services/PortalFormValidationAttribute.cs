using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EnjoySpace.Web.Services;

// Opt in on student registration, profile and trial forms. Runs before ApiController's
// automatic ModelStateInvalidFilter (-2000), including JSON conversion errors.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PortalFormValidationAttribute : ActionFilterAttribute
{
    public PortalFormValidationAttribute() => Order = -3000;

    private static readonly Dictionary<string, string> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Account"] = "帳號", ["Password"] = "密碼", ["ConfirmPassword"] = "確認密碼",
        ["Name"] = "姓名", ["Email"] = "電子郵件", ["Phone"] = "手機",
        ["LineId"] = "LINE 帳號", ["Note"] = "備註", ["ReferrerName"] = "介紹人姓名",
        ["SessionId"] = "體驗場次", ["PlanId"] = "體驗方案", ["SessionIds"] = "預約場次",
        ["SubmissionKey"] = "申請識別碼", ["BatchKey"] = "預約識別碼", ["RowVersion"] = "資料版本",
        ["Reason"] = "取消原因", ["id"] = "申請編號"
    };

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;
        var errors = context.ModelState.Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(error =>
                // Only our explicit Chinese validation messages are public. Framework
                // exceptions may contain submitted values, parser paths or English text.
                error.Exception == null && Regex.IsMatch(error.ErrorMessage, @"^[\u3400-\u9FFF]")
                    ? error.ErrorMessage : InvalidFormat(x.Key)).Distinct().ToArray());
        context.Result = new BadRequestObjectResult(new
        {
            message = "請確認欄位資料後再送出。",
            errors
        });
    }

    private static string InvalidFormat(string key)
    {
        var field = Regex.Replace(key.Split('.').LastOrDefault() ?? "", @"\[.*\]", "");
        if (field.Equals("RowVersion", StringComparison.OrdinalIgnoreCase))
            return "資料版本格式不正確，請重新整理頁面後再試。";
        if (field.Equals("SubmissionKey", StringComparison.OrdinalIgnoreCase) || field.Equals("BatchKey", StringComparison.OrdinalIgnoreCase))
            return "申請識別碼格式不正確，請重新整理頁面後再試。";
        return Fields.TryGetValue(field, out var label)
            ? $"{label}格式不正確，請確認後重新輸入。"
            : "表單資料格式不正確或未填寫，請確認後再送出。";
    }
}
