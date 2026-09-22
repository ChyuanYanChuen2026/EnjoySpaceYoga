using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Serviece;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class CourseService
{
    private readonly AppDbContext _db;
    public CourseService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<List<Course>> GetAllAsync()
    {
        return await _db.Courses.AsNoTracking().OrderByDescending(course => course.StartDate).ThenBy(course => course.Id).ToListAsync();
    }

    public async Task CreateAsync(string name, DateOnly startDate, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new ArgumentException("課程名稱必填且最多 100 個字元。");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("結束日期不得早於開始日期。");
        }

        var course = new Course
        {
            Name = name.Trim(),
            StartDate = startDate,
            EndDate = endDate
        };

        _db.Courses.Add(course);

        await _db.SaveChangesAsync();
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _db.Courses.AsNoTracking().SingleOrDefaultAsync(course => course.Id == id);
    }

    public async Task<CourseUpdateResult>UpdateAsync(int id, string name, DateOnly startDate, DateOnly endDate, byte[] rowVersion)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new ArgumentException("課程名稱必填且最多 100 個字元。");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("結束日期不得早於開始日期。");
        }

        if (rowVersion is null || rowVersion.Length != 8)
        {
            throw new ArgumentException("資料版本無效。");
        }

        var course = await _db.Courses.SingleOrDefaultAsync(course => course.Id == id);

        if (course is null)
        {
            return CourseUpdateResult.NotFound;
        }

        if (!course.RowVersion.SequenceEqual(rowVersion))
        {
            return CourseUpdateResult.Conflict;
        }

        _db.Entry(course).Property(course => course.RowVersion).OriginalValue = rowVersion;

        course.Name = name.Trim();
        course.StartDate = startDate;
        course.EndDate = endDate;

        try
        {
            await _db.SaveChangesAsync();

            return CourseUpdateResult.Saved;
        }
        catch (DbUpdateConcurrencyException)
        {
            _db.Entry(course).State = EntityState.Detached;

            return CourseUpdateResult.Conflict;
        }
    }
}