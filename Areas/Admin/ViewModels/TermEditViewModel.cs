using System.ComponentModel.DataAnnotations;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TermEditViewModel : TermCreateViewModel
{
    [Range(1, long.MaxValue, ErrorMessage = "學期編號不正確")]
    public long TermId { get; set; }

    [Required(ErrorMessage = "請選擇學期狀態")]
    [Display(Name = "學期狀態")]
    public string Status { get; set; } = TermStatusOptions.Draft;

    [Required(ErrorMessage = "缺少資料版本，請重新開啟編輯頁")]
    [MinLength(1, ErrorMessage = "缺少資料版本，請重新開啟編輯頁")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}