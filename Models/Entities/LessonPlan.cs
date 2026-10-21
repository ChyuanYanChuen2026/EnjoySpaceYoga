using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("LessonPlans", Schema = "dbo")]
public class LessonPlan
{
    public long LessonPlanId { get; set; }
    [MaxLength(50)] public string PlanCode { get; set; } = "";
    [MaxLength(100)] public string PlanName { get; set; } = "";
    [MaxLength(30)] public string PlanType { get; set; } = "Trial";
    [MaxLength(30)] public string AudienceType { get; set; } = "TrialGuest";
    public short SessionQuota { get; set; } = 1;
    [Column(TypeName = "decimal(10,2)")] public decimal Price { get; set; }
    [MaxLength(20)] public string ValidityUnit { get; set; } = "Days";
    public short? ValidityValue { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public bool RequiresReferrer { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime CreatedAtUtc { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
}
