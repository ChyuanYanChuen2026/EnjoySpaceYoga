using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Payments", Schema = "dbo")]
public class Payment
{
    public long PaymentId { get; set; }
    public long? StudentId { get; set; }
    public long? TrialGuestId { get; set; }
    [MaxLength(50)] public string PaymentPurpose { get; set; } = "CourseEnrollment";
    [Column(TypeName = "decimal(10,2)")] public decimal Amount { get; set; }
    [MaxLength(30)] public string? PaymentMethod { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    [Column(TypeName = "datetime2(0)")] public DateTime? PaidAtUtc { get; set; }
    public long? ConfirmedByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? ConfirmedAtUtc { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime CreatedAtUtc { get; set; }
    public Student? Student { get; set; }
    public TrialGuest? TrialGuest { get; set; }
    public User? ConfirmedByUser { get; set; }
}
