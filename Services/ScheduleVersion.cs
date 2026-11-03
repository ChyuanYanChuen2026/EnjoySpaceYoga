using EnjoySpace.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

internal static class ScheduleVersion
{
    public static async Task RequireCurrentAsync(AppDbContext db, long termId, byte[] version)
    {
        var term = await db.Terms.AsNoTracking().SingleOrDefaultAsync(x => x.TermId == termId);
        if (term is null || term.Status != "Draft" || version.Length != 8 || !term.RowVersion.SequenceEqual(version))
            throw new InvalidOperationException("課表已變更，請重新載入此步驟後再試。");
    }

    // SQL Server advances RowVersion even when the assigned value remains null.
    // Call inside the mutation's transaction so a previously opened release form becomes stale.
    public static async Task TouchDraftAsync(AppDbContext db, long termId)
    {
        var updated = await db.Terms.Where(x => x.TermId == termId && x.Status == "Draft")
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.ScheduleConfirmedAtUtc, (DateTime?)null));
        if (updated != 1) throw new InvalidOperationException("學期已離開草稿，請重新整理課表。");
    }
}
