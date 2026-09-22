using System.ComponentModel.DataAnnotations;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class CourseCreateViewModel : IValidatableObject
{
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