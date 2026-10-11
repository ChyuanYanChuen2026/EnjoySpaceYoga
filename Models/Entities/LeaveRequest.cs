using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("LeaveRequests", Schema = "dbo")]
public class LeaveRequest
{
    public long LeaveRequestId { get; set; }
    public long SessionParticipantId { get; set; }
    [MaxLength(30)] public string RequestSource { get; set; } = "Admin";
    [MaxLength(500)] public string Reason { get; set; } = "";
    [MaxLength(30)] public string Status { get; set; } = "Active";
    public long CreatedByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime RequestedAtUtc { get; set; }
    public long? CancelledByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? CancelledAtUtc { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public ClassSessionParticipant Participant { get; set; } = null!;
    public MakeupEntitlement Entitlement { get; set; } = null!;
}
