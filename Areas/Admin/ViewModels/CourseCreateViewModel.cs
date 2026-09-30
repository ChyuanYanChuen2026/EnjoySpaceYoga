using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class CourseCreateViewModel : IValidatableObject
{
    [Display(Name = "學期")]
    [Required(ErrorMessage = "請選擇學期")]
    [Range(typeof(long),"1", "9223372036854775807", ErrorMessage = "學期編號無效")]
    public long? TermId { get; set; }
    public List<SelectListItem> TermOptions { get; set; } = new();

    [Display(Name = "課程類別")]
    [Required(ErrorMessage = "請選擇課程類別")]
    [Range(1, int.MaxValue, ErrorMessage = "課程分類編號無效")]
    public int? ClassCategoryId { get; set; }
    public List<SelectListItem> ClassCategoryOptions { get; set; } = new();

    [Display(Name = "預設老師")]
    [Required(ErrorMessage = "請選擇預設老師")]
    [Range(typeof(long), "1", "9223372036854775807", ErrorMessage = "老師編號無效")]
    public long? DefaultTeacherId { get; set; }
    public List<SelectListItem> TeacherOptions { get; set; } = new();

    [Display(Name = "預設地點")]
    [Required(ErrorMessage = "請選擇預設地點")]
    [Range(typeof(long),  "1",  "9223372036854775807", ErrorMessage = "地點編號無效")]
    public long? DefaultLocationId { get; set; }
    public List<SelectListItem> LocationOptions { get; set; } = new();

    [Display(Name = "課程名稱")]
    [Required(ErrorMessage = "請輸入課程名稱")]
    [StringLength(150, ErrorMessage = "課程名稱最多 150 個字元")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "預設名額")]
    [Required(ErrorMessage = "請輸入預設名額")]
    [Range(1, short.MaxValue, ErrorMessage = "預設名額須介於 1～32767")]
    public short? DefaultCapacity { get; set; } = 10;

    [Display(Name = "整期期課價格（元）")]
    [Required(ErrorMessage = "請輸入期課價格")]
    [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "價格須介於 0～99999999.99")]
    public decimal? TermPrice { get; set; }

    [Display(Name = "公開顯示")]
    public bool IsPubliclyVisible { get; set; } = true;
    [Display(Name = "允許自由預約")]
    public bool AllowsFlexibleBooking { get; set; } = true;
    [Display(Name = "允許補課")]
    public bool AllowsMakeup { get; set; } = true;
    [Display(Name = "啟用課程")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TermPrice.HasValue && decimal.Round(TermPrice.Value, 2) != TermPrice.Value)
            yield return new ValidationResult("價格最多兩位小數", new[] { nameof(TermPrice) });
    }
}
