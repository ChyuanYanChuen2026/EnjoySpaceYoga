using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("CourseEnrollments", Schema = "dbo")]
public class CourseEnrollment
{
    public long CourseEnrollmentId { get; set; }
    public long CourseId { get; set; }
    public long StudentId { get; set; }
    public long EnrollmentApplicationId { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Active";
    [Column(TypeName = "datetime2(0)")] public DateTime EnrolledAtUtc { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? CancelledAtUtc { get; set; }
    public long CreatedByUserId { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    public Course Course { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public EnrollmentApplication EnrollmentApplication { get; set; } = null!;
}
