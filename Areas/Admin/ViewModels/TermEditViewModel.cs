using System.ComponentModel.DataAnnotations;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TermEditViewModel : TermCreateViewModel
{
    [Range(1, long.MaxValue, ErrorMessage = "學期編號不正確")]
    public long TermId { get; set; }

    [Required(ErrorMessage = "請選擇學期狀態")]
    [Display(Name = "學期狀態")]
    public string Status { get; set; } = TermStatusOptions.Draft;

    [Display(Name = "我已核對本學期所有啟用課程的完整課表，確認排課完成後統一開放報名")]
    public bool ConfirmScheduleComplete { get; set; }
    public DateTime? ScheduleConfirmedAtUtc { get; set; }
    public List<TermCourseScheduleItem> Schedule { get; set; } = [];

    [Required(ErrorMessage = "缺少資料版本，請重新開啟編輯頁")]
    [MinLength(1, ErrorMessage = "缺少資料版本，請重新開啟編輯頁")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public class TermCourseScheduleItem
{
    public long CourseId { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public int SessionCount { get; set; }
    public DateTimeOffset? FirstSessionAt { get; set; }
    public DateTimeOffset? LastSessionAt { get; set; }
}
