using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

public static class AdminAccountSetup
{
    public static async Task CreateAsync(AppDbContext db, string account)
    {
        if (string.IsNullOrWhiteSpace(account) || account.Length > 100) throw new InvalidOperationException("請提供 100 字內的管理員帳號。");
        account = account.Trim();
        if (await db.Users.AnyAsync(x => x.Account == account))  throw new InvalidOperationException("此帳號已存在；不會覆蓋既有密碼或提升既有帳號權限。");
        Console.Write("請輸入新管理員密碼（12～200 字元，不會顯示）：");
        var password = ReadPassword();
        Console.Write("再次輸入密碼：");
        var confirmation = ReadPassword();
        await CreateAccountAsync(db, account, password, confirmation);
        Console.WriteLine("管理員已建立，請至 /Account/Login 登入。");
    }

    // 共用既有帳號與雜湊流程；由呼叫者決定交易範圍及提交後的成功提示。
    public static async Task<User> CreateAccountAsync(AppDbContext db, string account, string password, string confirmation)
    {
        if (string.IsNullOrWhiteSpace(account) || account.Length > 100)
            throw new InvalidOperationException("請提供 100 字內的管理員帳號。");
        account = account.Trim();
        if (password.Length is < 12 or > 200 || password != confirmation)
            throw new InvalidOperationException("密碼須為 12～200 字元，且兩次輸入相同。");
        if (await db.Users.AnyAsync(x => x.Account == account))
            throw new InvalidOperationException("此帳號已存在；不會覆蓋既有密碼或提升既有帳號權限。");
        var role = await db.Roles.SingleOrDefaultAsync(x => x.RoleCode == "Admin" && x.IsActive)
            ?? throw new InvalidOperationException("找不到啟用中的 Admin 角色，請先確認資料庫初始化結果。");
        var user = new User { Account = account, DisplayName = account, IsActive = true };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.UserRoles.Add(new UserRole { User = user, RoleId = role.RoleId });
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task ResetPasswordAsync(AppDbContext db, string account)
    {
        var user = await db.Users.SingleOrDefaultAsync( u => u.Account == account.Trim());
        if (user is null) throw new InvalidOperationException("找不到此帳號。");
        Console.WriteLine($"即將重設帳號：{user.Account}");
        Console.Write("請輸入新密碼（12～200 字元，不會顯示）：");
        var password = ReadPassword();
        Console.Write("再次輸入新密碼：");
        var confirmation = ReadPassword();
        if (password.Length is < 12 or > 200 || password != confirmation)
            throw new InvalidOperationException("密碼須為 12～200 字元，且兩次輸入相同。");
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        Console.WriteLine($"帳號 {user.Account} 密碼已重設，帳號啟用狀態未變更。");
    }
    private static string ReadPassword()
    {
        if (Console.IsInputRedirected) throw new InvalidOperationException("請在互動式終端機執行管理員建立指令。");
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) { Console.WriteLine(); return password.ToString(); }
            if (key.Key == ConsoleKey.Backspace && password.Length > 0) password.Length--;
            else if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
        }
    }
}
