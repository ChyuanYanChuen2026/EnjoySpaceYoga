using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("TrialGuests", Schema = "dbo")]
public class TrialGuest
{
    public long TrialGuestId { get; set; }
    [Timestamp] public byte[] RowVersion { get; set; } = [];
    [MaxLength(100)] public string Name { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(100)] public string LineId { get; set; } = "";
    [MaxLength(30)] public string SourceType { get; set; } = "";
    public long? ReferrerStudentId { get; set; }
    public long? ConvertedStudentId { get; set; }
    [MaxLength(30)] public string Status { get; set; } = "Active";
    [MaxLength(500)] public string? Note { get; set; }
    public long? CreatedByUserId { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime CreatedAtUtc { get; set; }
    [Column(TypeName = "datetime2(0)")] public DateTime? UpdatedAtUtc { get; set; }
}
