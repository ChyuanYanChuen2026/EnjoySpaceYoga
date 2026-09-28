using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class EnrollmentApplicationService
{
    private readonly AppDbContext _db;

    public EnrollmentApplicationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<EnrollmentApplicationListItem>> GetPendingAsync()
    {
        return await _db.EnrollmentApplications
            .AsNoTracking()
            .Where(application =>
                application.Status == EnrollmentApplicationStatus.Pending)
            .OrderBy(application => application.SubmittedAt)
            .ThenBy(application => application.Id)
            .Select(application => new EnrollmentApplicationListItem
            {
                Id = application.Id,
                StudentName = application.Student.User!.DisplayName,
                Phone = application.Student.User!.Phone,
                Email = application.Student.User!.Email,
                CourseName = application.Course.Name,
                SubmittedAt = application.SubmittedAt
            })
            .ToListAsync();
    }
}