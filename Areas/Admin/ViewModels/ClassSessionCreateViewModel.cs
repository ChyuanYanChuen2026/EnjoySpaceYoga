using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class ClassSessionCreateViewModel : IValidatableObject
{
    [Display(Name = "課程")]
    [Range(1, int.MaxValue, ErrorMessage = "請選擇課程")]
    public int CourseId { get; set; }

    [Display(Name = "老師")]
    [Range(1, int.MaxValue, ErrorMessage = "請選擇老師")]
    public int TeacherId { get; set; }

    [Display(Name = "地點")]
    [Range(1, int.MaxValue, ErrorMessage = "請選擇地點")]
    public int LocationId { get; set; }

    [Display(Name = "開始時間（台灣）")]
    [Required(ErrorMessage = "請選擇開始時間")]
    public DateTime? StartsAt { get; set; }

    [Display(Name = "結束時間（台灣）")]
    [Required(ErrorMessage = "請選擇結束時間")]
    public DateTime? EndsAt { get; set; }

    public List<SelectListItem> CourseOptions { get; set; } = new();

    public List<SelectListItem> TeacherOptions { get; set; } = new();

    public List<SelectListItem> LocationOptions { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartsAt.HasValue && EndsAt.HasValue && EndsAt.Value <= StartsAt.Value)
        {
            yield return new ValidationResult("結束時間必須晚於開始時間", new[] { nameof(EndsAt) });
        }
    }
}