namespace EnjoySpace.Web.Services;

public static class TaiwanMobile
{
    // Shared by MVC/JSON validation and the HTML pattern on student forms.
    // Accept only ten ASCII digits; separators must not be silently removed.
    public const string InputPattern = "09[0-9]{8}";
    public const string ErrorMessage = "請輸入 09 開頭的 10 碼手機號碼，例如 0912345678（不含空格或符號）。";
    public const string InputHint = "請輸入 09 開頭的 10 碼手機號碼，例如 0912345678，不含空格或符號。";
}
