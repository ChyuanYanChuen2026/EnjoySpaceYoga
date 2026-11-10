using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

// Maps the existing audit table; recording a change is a separate workflow.
[Table("SessionTeacherChanges", Schema = "dbo")]
public class SessionTeacherChange
{
    public long Id { get; set; }
    public int ClassSessionId { get; set; }
    public long PreviousTeacherId { get; set; }
    public long TeacherId { get; set; }
    public long ChangedByUserId { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
    public ClassSession ClassSession { get; set; } = null!;
    public Teacher PreviousTeacher { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
