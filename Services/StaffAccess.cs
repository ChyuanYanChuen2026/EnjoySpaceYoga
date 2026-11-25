using EnjoySpace.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public static class StaffAccess
{
    public const string ReadOnlyRole = "AdminReadOnly";

    public static async Task<List<string>> RolesAsync(AppDbContext db, long userId)
    {
        var roles = await db.UserRoles
            .Where(x => x.UserId == userId && x.User.IsActive && x.Role.IsActive &&
                (x.Role.RoleCode == "Admin" || x.Role.RoleCode == ReadOnlyRole ||
                 (x.Role.RoleCode == "Teacher" && db.Teachers.Any(t => t.UserId == userId && t.IsActive))))
            .Select(x => x.Role.RoleCode).Distinct().ToListAsync();
        return roles.Contains(ReadOnlyRole) ? [ReadOnlyRole] : roles;
    }
}
