namespace EnjoySpace.Web.Models;

public class ClassSessionDetailModel
{
    public int Id { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public DateTimeOffset StartsAt { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public int Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public bool IsExtraSession { get; set; }

    public int? CapacityOverride { get; set; }

    public int EffectiveCapacity => CapacityOverride ?? Capacity;
    public byte[] RowVersion { get; set; } = [];
    public string TermStatus { get; set; } = "";
    public bool HasApplications { get; set; }
    public bool CanDelete => !IsExtraSession && TermStatus == "Draft" && !HasApplications;
}
