using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Users", Schema = "dbo")]
public class User
{
    public long UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string Account { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(100)]
    public string? LineId { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAtUtc { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? UpdatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}