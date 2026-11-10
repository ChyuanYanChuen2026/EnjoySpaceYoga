namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TeacherListItemViewModel
{
    public long TeacherId { get; set; }

    public string? TeacherNo { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public bool IsUserActive { get; set; }
}