using EnjoySpace.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

internal static class ScheduleVersion
{
    // SQL Server advances RowVersion even when the assigned value remains null.
    // Call inside the mutation's transaction so a previously opened release form becomes stale.
    public static async Task TouchDraftAsync(AppDbContext db, long termId)
    {
        var updated = await db.Terms.Where(x => x.TermId == termId && x.Status == "Draft")
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.ScheduleConfirmedAtUtc, (DateTime?)null));
        if (updated != 1) throw new InvalidOperationException("學期已離開草稿，請重新整理課表。");
    }
}
