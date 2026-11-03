using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public record TrialReviewSummary(int Plans, int Bookings)
{
    public int Total => Plans + Bookings;
}

// Scoped: the navigation and page share one summary within this request.
public class TrialReviewSummaryService(AppDbContext db)
{
    private TrialReviewSummary? summary;
    public IQueryable<PortalTrialRequest> PendingPlans => db.PortalTrialRequests
        .Where(x => x.Status == "Pending" || x.Status == "Contacted");
    public IQueryable<TrialBooking> PendingBookings => db.TrialBookings.Where(x => x.Status == "Pending");

    public async Task<TrialReviewSummary> GetAsync() => summary ??= new(
        await PendingPlans.CountAsync(), await PendingBookings.CountAsync());
}
