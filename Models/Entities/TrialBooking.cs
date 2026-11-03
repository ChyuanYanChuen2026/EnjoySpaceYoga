using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EnjoySpace.Web.Models.Entities;

[Table("TrialBookings", Schema = "dbo")]
public class TrialBooking
{
    public long Id { get; set; }
    public long TrialRequestId { get; set; }
    public PortalTrialRequest TrialRequest { get; set; } = null!;
    public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public Guid BatchKey { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    public DateTimeOffset CreatedAt { get; set; }
    public long? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    [MaxLength(500)] public string? ReviewNote { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
}
