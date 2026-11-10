using EnjoySpace.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public static class StaffAccess
{
    public static Task<List<string>> RolesAsync(AppDbContext db, long userId) => db.UserRoles
        .Where(x => x.UserId == userId && x.User.IsActive && x.Role.IsActive &&
            (x.Role.RoleCode == "Admin" || (x.Role.RoleCode == "Teacher" && db.Teachers.Any(t => t.UserId == userId && t.IsActive))))
        .Select(x => x.Role.RoleCode).Distinct().ToListAsync();
}
