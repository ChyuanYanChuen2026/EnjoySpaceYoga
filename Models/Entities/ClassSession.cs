using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("ClassSessions", Schema = "dbo")]
public class ClassSession
{
    public int Id { get; set; }

    public long CourseId { get; set; }

    public long TeacherId { get; set; }

    public long StudioLocationId { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; } = 10;

    [Range(1, int.MaxValue)]
    public int? CapacityOverride { get; set; }

    public bool IsExtraSession { get; set; }

    public bool IsCancelled { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public long? CancelledByUserId { get; set; }
    public User? CancelledByUser { get; set; }
    [MaxLength(500)] public string? CancellationReason { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Course Course { get; set; } = null!;

    public Teacher Teacher { get; set; } = null!;

    public StudioLocation StudioLocation { get; set; } = null!;

}
