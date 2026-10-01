using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Roles", Schema = "dbo")]
public class Role
{
    public int RoleId { get; set; }

    [Required]
    [StringLength(50)]
    public string RoleCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}