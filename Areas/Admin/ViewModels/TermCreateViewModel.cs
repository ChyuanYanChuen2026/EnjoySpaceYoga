using System.ComponentModel.DataAnnotations;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TermCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "請輸入學期名稱")]
    [StringLength(100, ErrorMessage = "學期名稱最多 100 字")]
    [Display(Name = "學期名稱")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "請選擇開始日期")]
    [DataType(DataType.Date)]
    [Display(Name = "開始日期")]
    public DateOnly? StartDate { get; set; }

    [Required(ErrorMessage = "請選擇結束日期")]
    [DataType(DataType.Date)]
    [Display(Name = "結束日期")]
    public DateOnly? EndDate { get; set; }

    [Display(Name = "報名開始時間（台灣時間）")]
    public DateTime? RegistrationStartAtTaiwan { get; set; }

    [Display(Name = "報名結束時間（台灣時間）")]
    public DateTime? RegistrationEndAtTaiwan { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue &&  EndDate.Value < StartDate.Value)
        {
            yield return new ValidationResult( "結束日期不可早於開始日期", new[] { nameof(EndDate) });
        }

        if (RegistrationStartAtTaiwan.HasValue && RegistrationEndAtTaiwan.HasValue && RegistrationEndAtTaiwan.Value <= RegistrationStartAtTaiwan.Value)
        {
            yield return new ValidationResult("報名結束時間必須晚於報名開始時間", new[] { nameof(RegistrationEndAtTaiwan) });
        }
    }
}