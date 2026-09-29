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

    [Display(Name = "課程名稱")]
    [Required(ErrorMessage = "請輸入課程名稱")]
    [StringLength(100, ErrorMessage = "課程名稱最多 100 個字元")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "開始日期")]
    [Required(ErrorMessage = "請選擇開始日期")]
    [DataType(DataType.Date)]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "結束日期")]
    [Required(ErrorMessage = "請選擇結束日期")]
    [DataType(DataType.Date)]
    public DateOnly? EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            yield return new ValidationResult("結束日期不得早於開始日期", new[] { nameof(EndDate) });
        }
    }
}