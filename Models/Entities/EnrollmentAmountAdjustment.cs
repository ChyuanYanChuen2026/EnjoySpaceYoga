using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("EnrollmentAmountAdjustments", Schema = "dbo")]
public class EnrollmentAmountAdjustment
{
    public long Id { get; set; }
    public long EnrollmentApplicationId { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal PreviousAmount { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
    public long AdjustedByUserId { get; set; }
    public DateTimeOffset AdjustedAt { get; set; }
}
