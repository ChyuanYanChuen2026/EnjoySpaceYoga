using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("StudioLocations", Schema = "dbo")]
public class StudioLocation
{
    public long StudioLocationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    public int CheckInRadiusMeters { get; set; } = 500;

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAtUtc { get; set; }
}