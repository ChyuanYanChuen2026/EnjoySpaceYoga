namespace EnjoySpace.Web.Models;

public static class LessonPlanLabels
{
    public static string Audience(string code) => code switch { "TrialGuest" => "體驗生", "Student" => "正式學生", _ => code };
    public static string Validity(string unit, short? value) => unit switch
    {
        "Days" => $"{value} 天（含生效日）",
        "Months" => $"{value} 個月",
        "Session" => "依堂次使用",
        "Unlimited" => "不限期",
        _ => "請洽工作室確認效期"
    };
}
