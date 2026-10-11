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
    public long TrialGuestId { get; set; }
    public TrialGuest TrialGuest { get; set; } = null!;
    public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public long LessonPlanId { get; set; }
    public LessonPlan LessonPlan { get; set; } = null!;
    [MaxLength(100)] public string PlanName { get; set; } = "";
    [Column(TypeName = "decimal(10,2)")] public decimal QuotedPrice { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    [MaxLength(300)] public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
