using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnjoySpace.Web.Models.Enums;

namespace EnjoySpace.Web.Models.Entities;

[Table("EnrollmentApplications", Schema = "dbo")]
public class EnrollmentApplication
{
    public int Id { get; set; }

    public long StudentId { get; set; }

    public int CourseId { get; set; }

    public EnrollmentApplicationStatus Status { get; set; }  = EnrollmentApplicationStatus.Pending;

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReviewedAt { get; set; }

    [StringLength(500)]
    public string? ReviewNote { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}