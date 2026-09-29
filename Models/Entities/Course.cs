using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Courses", Schema = "dbo")]
public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public long TermId { get; set; }

    public Term Term { get; set; } = null!;

    public int ClassCategoryId { get; set; }

    public ClassCategory ClassCategory { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

