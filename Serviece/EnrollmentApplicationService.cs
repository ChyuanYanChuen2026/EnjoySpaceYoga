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
                StudentName = application.Student.Name,
                Phone = application.Student.Phone,
                Email = application.Student.Email,
                CourseName = application.Course.Name,
                SubmittedAt = application.SubmittedAt
            })
            .ToListAsync();
    }
}