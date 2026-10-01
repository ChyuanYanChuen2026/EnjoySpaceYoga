using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

public static class AdminAccountSetup
{
    public static async Task CreateAsync(AppDbContext db, string account)
    {
        if (string.IsNullOrWhiteSpace(account) || account.Length > 100)
            throw new InvalidOperationException("請提供 100 字內的管理員帳號。");
        if (await db.Users.AnyAsync(x => x.Account == account))
            throw new InvalidOperationException("此帳號已存在；不會覆蓋既有密碼或提升既有帳號權限。");
        Console.Write("請輸入新管理員密碼（至少 12 字元，不會顯示）：");
        var password = ReadPassword();
        Console.Write("再次輸入密碼：");
        var confirmation = ReadPassword();
        if (password.Length < 12 || password != confirmation)
            throw new InvalidOperationException("密碼須至少 12 字元，且兩次輸入相同。");
        var role = await db.Roles.SingleAsync(x => x.RoleCode == "Admin" && x.IsActive);
        var user = new User { Account = account.Trim(), DisplayName = account.Trim(), IsActive = true };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.UserRoles.Add(new UserRole { User = user, RoleId = role.RoleId });
        await db.SaveChangesAsync();
        Console.WriteLine("管理員已建立，請至 /Account/Login 登入。");
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
