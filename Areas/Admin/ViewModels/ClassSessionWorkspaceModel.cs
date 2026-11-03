using EnjoySpace.Web.Models;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class ClassSessionWorkspaceModel
{
    public required ClassSessionDetailModel Session { get; init; }
    public required AttendanceRosterViewModel Roster { get; init; }
}
