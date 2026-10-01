using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Teachers", Schema = "dbo")]
public class Teacher
{
    public long TeacherId { get; set; }

    public long UserId { get; set; }

    public User User { get; set; } = null!;

    [StringLength(30)]
    public string? TeacherNo { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Note { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAtUtc { get; set; }

}