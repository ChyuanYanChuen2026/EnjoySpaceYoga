using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Models;

public class TeacherRequestInput
{
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();
    [Required] public string Kind { get; set; } = "ExtraSession";
    [Range(1, long.MaxValue)] public long TeacherId { get; set; }
    public long CourseId { get; set; }
    public int? TargetSessionId { get; set; }
    public long StudioLocationId { get; set; }
    public DateTime? StartsAtTaiwan { get; set; }
    public DateTime? EndsAtTaiwan { get; set; }
    [Range(1, 32767)] public int RequestedCapacity { get; set; } = 10;
    [Required, StringLength(500)] public string Reason { get; set; } = "";
    public List<SelectListItem> Teachers { get; set; } = [];
    public List<SelectListItem> Courses { get; set; } = [];
    public List<SelectListItem> Locations { get; set; } = [];
    public List<SelectListItem> Sessions { get; set; } = [];
}

public class TeacherRequestReview
{
    public long Id { get; set; }
    [Required] public byte[] RowVersion { get; set; } = [];
    [Required] public string Decision { get; set; } = "";
    [StringLength(500)] public string? Note { get; set; }
    public bool ConfirmDetails { get; set; }
}
