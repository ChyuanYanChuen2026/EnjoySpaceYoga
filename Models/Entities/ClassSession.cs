using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("ClassSessions", Schema = "dbo")]
public class ClassSession
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public int LocationId { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; } = 10;

    [Range(1, int.MaxValue)]
    public int? CapacityOverride { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Course Course { get; set; } = null!;

    public Teacher Teacher { get; set; } = null!;

    public Location Location { get; set; } = null!;
}