using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace EnjoySpace.Web.Models.Entities;

[Table("SessionQrCodes", Schema = "dbo")]
public class SessionQrCode
{
    [Key] public int ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    [MaxLength(64)] public string TokenHash { get; set; } = "";
    [MaxLength(1024)] public string ProtectedToken { get; set; } = "";
    public DateTimeOffset OpensAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

[Table("CheckInEvents", Schema = "dbo")]
public class CheckInEvent
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public int? ClassSessionId { get; set; }
    public ClassSession? ClassSession { get; set; }
    [MaxLength(40)] public string Code { get; set; } = "";
    [MaxLength(1024)] public string Message { get; set; } = "";
    [MaxLength(30)] public string? AttendanceStatus { get; set; }
    public bool IsError { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? ResolvedByUserId { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    [MaxLength(500)] public string? Resolution { get; set; }
}
