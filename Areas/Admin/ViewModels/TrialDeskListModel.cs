using EnjoySpace.Web.Models.Entities;

namespace EnjoySpace.Web.Areas.Admin.ViewModels;

public class TrialDeskListModel : PagedRecords<PortalTrialRequest>
{
    public string Review { get; set; } = "all";
    public Dictionary<long, int> PendingBookings { get; set; } = [];
}
