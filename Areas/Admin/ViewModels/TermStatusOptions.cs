namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public static class TermStatusOptions
{
    public const string Draft = "Draft";
    public const string Open = "Open";
    public const string Closed = "Closed";
    public const string Archived = "Archived";

    public static readonly IReadOnlyList<(string Value, string Text)> Items =
    [
        (Draft, "草稿"),
        (Open, "開放報名"),
        (Closed, "停止報名"),
        (Archived, "封存")
    ];

    public static bool IsValid(string? status) => Items.Any(item => item.Value == status);

    public static string GetText(string? status) => Items.FirstOrDefault(item => item.Value == status).Text ?? status ?? "未設定";

    public static bool CanChangeTo(string currentStatus, string nextStatus)
    {
        if (currentStatus == nextStatus) return true;

        return (currentStatus, nextStatus) switch
        {
            (Draft, Open) => true,
            (Draft, Archived) => true,

            (Open, Closed) => true,

            (Closed, Open) => true,
            (Closed, Draft) => true,
            (Closed, Archived) => true,

            _ => false
        };
    }
}
