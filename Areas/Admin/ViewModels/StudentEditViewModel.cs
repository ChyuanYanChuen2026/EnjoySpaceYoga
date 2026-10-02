using System.ComponentModel.DataAnnotations;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class StudentEditViewModel : StudentCreateViewModel
{
    [Range(1, long.MaxValue, ErrorMessage = "學生編號不正確")]
    public long StudentId { get; set; }

    [Required(ErrorMessage = "缺少資料版本，請重新開啟編輯頁")]
    [MinLength(1, ErrorMessage = "缺少資料版本，請重新開啟編輯頁")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}