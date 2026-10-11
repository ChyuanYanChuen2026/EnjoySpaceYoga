using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("ClassSessionParticipants", Schema = "dbo")]
public class ClassSessionParticipant
{
    [Key] public long SessionParticipantId { get; set; }
    // The running MVC schema uses int ClassSessions.Id (the design SQL uses bigint).
    public int ClassSessionId { get; set; }
    public long StudentId { get; set; }
    public long CourseEnrollmentId { get; set; }
    public long? MakeupRequestId { get; set; }
    public MakeupRequest? MakeupRequest { get; set; }
    [MaxLength(30)] public string ParticipationType { get; set; } = "Regular";
    [MaxLength(30)] public string Status { get; set; } = "Active";
    public long AddedByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime AddedAtUtc { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public ClassSession ClassSession { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public CourseEnrollment CourseEnrollment { get; set; } = null!;
    public AttendanceRecord Attendance { get; set; } = new();
}
