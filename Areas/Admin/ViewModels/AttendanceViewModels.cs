using System.ComponentModel.DataAnnotations;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public static class AttendanceStatuses
{
    public static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>
    {
        ["Scheduled"] = "未點名", ["Present"] = "出席", ["Late"] = "遲到", ["Absent"] = "缺席"
    };
}

public class AttendanceRosterViewModel
{
    public int SessionId { get; set; }
    public string CourseName { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string LocationName { get; set; } = "";
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public bool HasStarted { get; set; }
    public List<AttendanceRowViewModel> Participants { get; set; } = [];
}

public class AttendanceRowViewModel
{
    public long ParticipantId { get; set; }
    public string StudentName { get; set; } = "";
    public string? StudentNo { get; set; }
    public string Status { get; set; } = "Scheduled";
    public bool IsActive { get; set; }
    public string ParticipantStatus { get; set; } = "Active";
    public string ParticipationType { get; set; } = "Regular";
    public byte[] RowVersion { get; set; } = [];
    public string? ManualReason { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class AttendanceEditViewModel
{
    [Range(1, int.MaxValue)] public int SessionId { get; set; }
    [Range(1, long.MaxValue)] public long ParticipantId { get; set; }
    [Required] public string Status { get; set; } = "";
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
    [StringLength(500)] public string? ManualReason { get; set; }
}
