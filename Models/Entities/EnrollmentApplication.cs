using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EnjoySpace.Web.Models.Enums;

namespace EnjoySpace.Web.Models.Entities;

[Table("EnrollmentApplications", Schema = "dbo")]
public class EnrollmentApplication
{
    [Column("EnrollmentApplicationId")]
    public long Id { get; set; }

    [MaxLength(30)] public string ApplicationType { get; set; } = "FullTerm";
    public int? FirstSessionId { get; set; }
    public short QuotedSessionCount { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal QuotedAmount { get; set; }
    public long? PaymentId { get; set; }
    public long? ReviewedByUserId { get; set; }
    public Payment? Payment { get; set; }

    public long StudentId { get; set; }

    public long CourseId { get; set; }

    public EnrollmentApplicationStatus Status { get; set; }  = EnrollmentApplicationStatus.Pending;

    public DateTimeOffset SubmittedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReviewedAt { get; set; }

    [StringLength(500)]
    public string? ReviewNote { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
