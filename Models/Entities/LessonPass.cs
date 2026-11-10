using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("LessonPasses", Schema = "dbo")]
public class LessonPass
{
    public long LessonPassId { get; set; }
    public Guid PurchaseKey { get; set; }
    public long LessonPlanId { get; set; }
    public long? StudentId { get; set; }
    public long? TrialGuestId { get; set; }
    public long PaymentId { get; set; }
    [MaxLength(100)] public string PlanNameAtPurchase { get; set; } = "";
    [MaxLength(30)] public string PlanTypeAtPurchase { get; set; } = "";
    public short SessionQuotaAtPurchase { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal AmountAtPurchase { get; set; }
    public DateOnly ValidFromDate { get; set; }
    public DateOnly? ValidUntilDate { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime PurchasedAtUtc { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    public long CreatedByUserId { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public LessonPlan LessonPlan { get; set; } = null!;
    public Student? Student { get; set; }
    public TrialGuest? TrialGuest { get; set; }
    public Payment Payment { get; set; } = null!;
}
