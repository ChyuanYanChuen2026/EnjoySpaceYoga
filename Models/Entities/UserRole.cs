using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("UserRoles", Schema = "dbo")]
public class UserRole
{
    public long UserId { get; set; }

    public int RoleId { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime AssignedAtUtc { get; set; }

    public long? AssignedByUserId { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;

    public User? AssignedByUser { get; set; }
}