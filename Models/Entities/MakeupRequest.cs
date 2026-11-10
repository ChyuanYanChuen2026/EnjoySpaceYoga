using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("MakeupRequests", Schema = "dbo")]
public class MakeupRequest
{
    public long MakeupRequestId { get; set; }
    public long MakeupEntitlementId { get; set; }
    public int TargetClassSessionId { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Pending";
    public long RequestedByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime RequestedAtUtc { get; set; }
    public long? ReviewedByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? ReviewedAtUtc { get; set; }
    [MaxLength(500)] public string? ReviewNote { get; set; }
    public long? CancelledByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? CancelledAtUtc { get; set; }
    [MaxLength(500)] public string? CancellationReason { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public MakeupEntitlement Entitlement { get; set; } = null!;
    public ClassSession TargetSession { get; set; } = null!;
}
