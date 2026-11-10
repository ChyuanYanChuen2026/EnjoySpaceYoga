using System.Data;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Tools.FirstAdminSetup;

public static class FirstAdminInitializer
{
    public static async Task<User> CreateAsync(AppDbContext db, string account, string password, string confirmation)
    {
        // 不執行 Migration；只接受已完整初始化、與工具相同版本的資料庫。
        var expected = db.Database.GetMigrations().ToArray();
        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
        if (expected.Length == 0 || !expected.SequenceEqual(applied) || db.Database.HasPendingModelChanges())
            throw new InvalidOperationException("資料庫 Migration 版本與工具不一致，已停止建立管理員。");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock
                @Resource=N'EnjoySpace.FirstAdminSetup', @LockMode=N'Exclusive',
                @LockOwner=N'Transaction', @LockTimeout=10000;
            IF @result < 0 THROW 51180, N'其他管理員初始化作業正在進行，已停止。', 1;
            """);
        // 停用帳號或停用角色的歷史指派也計入，避免把此工具當成權限重設入口。
        if (await db.UserRoles.AnyAsync(x => x.Role.RoleCode == "Admin"))
            throw new InvalidOperationException("資料庫已有管理員權限紀錄，首位管理員工具不會再次建立或覆蓋帳號。");

        var user = await AdminAccountSetup.CreateAccountAsync(db, account, password, confirmation);
        await transaction.CommitAsync();
        return user;
    }
}
