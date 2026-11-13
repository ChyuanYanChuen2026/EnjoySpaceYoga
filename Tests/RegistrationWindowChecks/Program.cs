using System.Text.Json;
using EnjoySpace.Web.Services;

var start = new DateTimeOffset(2030, 1, 1, 2, 0, 0, TimeSpan.Zero);
var end = start.AddDays(1);
var count = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new Exception("失敗：" + label);
    Console.WriteLine("通過：" + label); count++;
}
Check(RegistrationWindow.UnavailableReason(start.UtcDateTime, end.UtcDateTime, start.AddTicks(-1))?.Contains("尚未到") == true, "開始前阻擋");
Check(RegistrationWindow.UnavailableReason(start.UtcDateTime, end.UtcDateTime, start) == null, "開始時間包含在內");
Check(RegistrationWindow.UnavailableReason(start.UtcDateTime, end.UtcDateTime, end.AddTicks(-1)) == null, "截止前允許");
Check(RegistrationWindow.UnavailableReason(start.UtcDateTime, end.UtcDateTime, end)?.Contains("截止") == true, "截止時間立即阻擋");
Check(RegistrationWindow.UnavailableReason(null, null, end) == null, "未設定時間保留既有語意");
Check(RegistrationWindow.UnavailableReason(null, end.UtcDateTime, end) != null, "單獨設定截止亦生效");
var data = new PortalCourseData { TermStatus = "Open", ScheduleConfirmed = true, FirstSessionAt = end.AddDays(10), HasEnrollmentCapacity = true,
    RegistrationStartAtUtc = RegistrationWindow.AsUtc(DateTime.SpecifyKind(start.UtcDateTime, DateTimeKind.Unspecified)), RegistrationEndAtUtc = end.UtcDateTime, ServerNowUtc = start };
Check(data.CanEnroll, "公開課程於開始時間可報名");
data.ServerNowUtc = end;
Check(!data.CanEnroll && data.EnrollmentUnavailableReason!.Contains("截止"), "公開課程截止狀態與後端共用判斷");
Check(JsonSerializer.Serialize(data).Contains("2030-01-01T02:00:00Z"), "API 序列化帶 UTC 標記");
Console.WriteLine($"全部 {count} 項檢查通過；未連接或修改資料庫。");
