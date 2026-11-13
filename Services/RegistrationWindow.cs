namespace EnjoySpace.Web.Services;

public static class RegistrationWindow
{
    // SQL datetime2 讀回的 Unspecified 仍表示 UTC，不可當成本機時間。
    public static DateTime? AsUtc(DateTime? value) => value.HasValue
        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

    public static string? UnavailableReason(DateTime? startAtUtc, DateTime? endAtUtc, DateTimeOffset now)
    {
        if (endAtUtc.HasValue && now.UtcDateTime >= AsUtc(endAtUtc)!.Value)
            return "此課程已截止報名，請聯繫工作室。";
        if (startAtUtc.HasValue && now.UtcDateTime < AsUtc(startAtUtc)!.Value)
            return "此課程尚未到報名開始時間。";
        return null;
    }
}
