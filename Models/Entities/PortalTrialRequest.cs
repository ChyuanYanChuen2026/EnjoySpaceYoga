using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("PortalTrialRequests", Schema = "dbo")]
public class PortalTrialRequest
{
    public long Id { get; set; }
    public Guid SubmissionKey { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    // Immutable submission details. NULL as a group identifies legacy requests without a snapshot.
    [MaxLength(100)] public string? SubmittedName { get; set; }
    [MaxLength(30)] public string? SubmittedPhone { get; set; }
    [MaxLength(200)] public string? SubmittedEmail { get; set; }
    [MaxLength(100)] public string? SubmittedLineId { get; set; }
    [MaxLength(100)] public string? SubmittedAccount { get; set; }
    public long TrialGuestId { get; set; }
    public TrialGuest TrialGuest { get; set; } = null!;
    public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public long LessonPlanId { get; set; }
    public LessonPlan LessonPlan { get; set; } = null!;
    [MaxLength(100)] public string PlanName { get; set; } = "";
    [Column(TypeName = "decimal(10,2)")] public decimal QuotedPrice { get; set; }
    public short? QuotedQuota { get; set; }
    [MaxLength(30)] public string? QuotedValidityUnit { get; set; }
    public int? QuotedValidityValue { get; set; }
    public bool? RequiresReferrerAtRequest { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    [MaxLength(300)] public string? Note { get; set; }
    [MaxLength(100)] public string? ReferrerName { get; set; }
    // Existing database fields restored from the schema inventory; no schema change here.
    public long? LessonPassId { get; set; }
    public LessonPass? LessonPass { get; set; }
    public long? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    [Column(TypeName = "datetime2")] public DateTime? ReviewedAtUtc { get; set; }
    [MaxLength(500)] public string? ReviewNote { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
}
