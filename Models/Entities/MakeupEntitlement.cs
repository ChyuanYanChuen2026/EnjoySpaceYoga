using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("MakeupEntitlements", Schema = "dbo")]
public class MakeupEntitlement
{
    public long MakeupEntitlementId { get; set; }
    public long StudentId { get; set; }
    public long SourceTermId { get; set; }
    public long SourceLeaveRequestId { get; set; }
    [MaxLength(30)] public string SourceType { get; set; } = "Leave";
    [MaxLength(30)] public string Status { get; set; } = "Available";
    [Column(TypeName = "datetime2(0)")] public DateTime CreatedAtUtc { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? ConsumedAtUtc { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? RevokedAtUtc { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public Student Student { get; set; } = null!;
    public Term SourceTerm { get; set; } = null!;
    public LeaveRequest SourceLeave { get; set; } = null!;
}
