using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Courses", Schema = "dbo")]
public class Course
{
    public long CourseId { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public long TermId { get; set; }

    public Term Term { get; set; } = null!;

    public int ClassCategoryId { get; set; }

    public ClassCategory ClassCategory { get; set; } = null!;

    public long DefaultTeacherId { get; set; }

    public Teacher DefaultTeacher { get; set; } = null!;

    public long DefaultLocationId { get; set; }

    public StudioLocation DefaultLocation { get; set; } = null!;

    [Range(1, short.MaxValue)]
    public short DefaultCapacity { get; set; } = 10;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TermPrice { get; set; }

    public bool IsPubliclyVisible { get; set; } = true;
    public bool AllowsFlexibleBooking { get; set; } = true;
    public bool AllowsMakeup { get; set; } = true;
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

