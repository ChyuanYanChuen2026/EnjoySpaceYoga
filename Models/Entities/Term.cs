using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnjoySpace.Web.Models.Entities;

[Table("Terms", Schema = "dbo")]
public class Term
{
    public long TermId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? RegistrationStartAtUtc { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime? RegistrationEndAtUtc { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Draft";

    [Column(TypeName = "datetime2(0)")]
    public DateTime? ScheduleConfirmedAtUtc { get; set; }
    public long? ScheduleConfirmedByUserId { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
