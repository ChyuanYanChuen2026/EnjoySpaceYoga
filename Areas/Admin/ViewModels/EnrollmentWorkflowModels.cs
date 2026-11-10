using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class EnrollmentCreateModel
{
    [Required, Range(1, long.MaxValue), Display(Name = "學生")] 
    public long? StudentId { get; set; }
    [Required, Range(1, long.MaxValue), Display(Name = "課程")] 
    public long? CourseId { get; set; }
    public List<SelectListItem> Students { get; set; } = [];
    public List<SelectListItem> Courses { get; set; } = [];
}
public class EnrollmentReviewModel
{
    public long Id { get; set; }
    [Required] public byte[] RowVersion { get; set; } = [];
    public bool Approve { get; set; }
    public bool ConfirmFinalReview { get; set; }
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
    public bool ConfirmStudentAgreement { get; set; }
}

public class EnrollmentConfirmationModel
{
    [Range(1, long.MaxValue)] public long StudentId { get; set; }
    [Range(1, long.MaxValue)] public long CourseId { get; set; }
    [Required] public string ConfirmationToken { get; set; } = "";
    public bool ConfirmDetails { get; set; }
    public bool ConfirmNoRefund { get; set; }
}

public class EnrollmentPreviewModel : EnrollmentConfirmationModel
{
    public string StudentName { get; set; } = "";
    public string StudentAccount { get; set; } = "";
    public string TermName { get; set; } = "";
    public string CourseName { get; set; } = "";
    public decimal Amount { get; set; }
    public List<EnrollmentSessionPreview> Sessions { get; set; } = [];
}

public class EnrollmentSessionPreview
{
    public int Id { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public string TeacherName { get; set; } = "";
    public string LocationName { get; set; } = "";
    public int Capacity { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
