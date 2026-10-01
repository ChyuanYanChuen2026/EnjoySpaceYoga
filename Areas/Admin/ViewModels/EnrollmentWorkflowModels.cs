using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class EnrollmentCreateModel
{
    [Required, Range(1, long.MaxValue), Display(Name = "學生")] public long? StudentId { get; set; }
    [Required, Range(1, long.MaxValue), Display(Name = "課程")] public long? CourseId { get; set; }
    public List<SelectListItem> Students { get; set; } = [];
    public List<SelectListItem> Courses { get; set; } = [];
}
public class EnrollmentReviewModel
{
    public long Id { get; set; }
    [Required] public byte[] RowVersion { get; set; } = [];
    public bool Approve { get; set; }
    [StringLength(500)] public string? Note { get; set; }
}
public class PaymentConfirmModel
{
    public long Id { get; set; }
    [Required] public byte[] RowVersion { get; set; } = [];
    [Required] public decimal? ReceivedAmount { get; set; }
    [Required] public string Method { get; set; } = "";
    [Required] public DateTime? PaidAtTaiwan { get; set; }
    [StringLength(500)] public string? Note { get; set; }
}
