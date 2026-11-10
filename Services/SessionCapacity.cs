using EnjoySpace.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

internal static class SessionCapacity
{
    // Every caller holds a serializable transaction. Lock the session before checking its active roster.
    public static async Task RequireSeatAsync(AppDbContext db, int sessionId, string? fullMessage = null)
    {
        var session = await db.ClassSessions.FromSqlInterpolated($"SELECT * FROM dbo.ClassSessions WITH (UPDLOCK, HOLDLOCK) WHERE Id = {sessionId}")
            .SingleAsync();
        var count = await db.ClassSessionParticipants.CountAsync(x => x.ClassSessionId == sessionId && x.Status == "Active");
        if (count >= (session.CapacityOverride ?? session.Capacity))
            throw new InvalidOperationException(fullMessage ?? "目標課堂名額已滿，請選擇其他場次或稍後再試。");
    }
}
