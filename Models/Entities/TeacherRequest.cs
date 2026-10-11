using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("TeacherRequests", Schema = "dbo")]
public class TeacherRequest
{
    public long Id { get; set; }
    public Guid SubmissionKey { get; set; }
    [MaxLength(20)] public string Kind { get; set; } = "ExtraSession";
    public long TeacherId { get; set; }
    public long CourseId { get; set; }
    public int? TargetSessionId { get; set; }
    public int? CreatedSessionId { get; set; }
    public long StudioLocationId { get; set; }
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public int RequestedCapacity { get; set; }
    public int? OriginalCapacity { get; set; }
    public byte[]? TargetVersion { get; set; }
    [MaxLength(500)] public string Reason { get; set; } = "";
    [MaxLength(20)] public string Status { get; set; } = "Pending";
    public long SubmittedByUserId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public long? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    [MaxLength(500)] public string? ReviewNote { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public Teacher Teacher { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public StudioLocation StudioLocation { get; set; } = null!;
    public ClassSession? TargetSession { get; set; }
    public ClassSession? CreatedSession { get; set; }
}
