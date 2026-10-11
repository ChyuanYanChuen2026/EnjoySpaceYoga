using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("AttendanceRecords", Schema = "dbo")]
public class AttendanceRecord
{
    public long AttendanceRecordId { get; set; }
    public long SessionParticipantId { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Scheduled";
    [MaxLength(30)] public string? CheckInMethod { get; set; }
    public long? CheckedInByUserId { get; set; }
    [MaxLength(500)] public string? ManualReason { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? UpdatedAtUtc { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public ClassSessionParticipant Participant { get; set; } = null!;
    public User? CheckedInByUser { get; set; }
}
