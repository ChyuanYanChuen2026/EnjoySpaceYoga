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

    private async Task ValidateTermAsync(long termId, DateOnly startDate, DateOnly endDate)
    {
        var term = await _db.Terms.AsNoTracking().SingleOrDefaultAsync(item => item.TermId == termId);
        if (term is null)
        {
            throw new ArgumentException("所選學期不存在，請重新選擇。");
        }
        if (startDate < term.StartDate || endDate > term.EndDate)
        {
            throw new ArgumentException($"課程日期必須位於學期期間 " + $"{term.StartDate:yyyy/MM/dd}～{term.EndDate:yyyy/MM/dd} 內。");
        }
    }

    public async Task<List<Term>> GetTermOptionsAsync()
    {
        return await _db.Terms
            .AsNoTracking()
            .OrderByDescending(term => term.StartDate)
            .ThenBy(term => term.TermId)
            .ToListAsync();
    }
    public async Task<List<Course>> GetAllAsync()
    {
        return await _db.Courses
        .AsNoTracking()
        .Include(course => course.Term)
        .OrderByDescending(course => course.StartDate)
        .ThenBy(course => course.Id)
        .ToListAsync();
    }

    public async Task CreateAsync(
    string name,
    long termId,
    int classCategoryId,
    DateOnly startDate,
    DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new ArgumentException("課程名稱必填且最多 100 個字元。");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("結束日期不得早於開始日期。");
        }
        await ValidateTermAsync(termId, startDate, endDate);
        await ValidateClassCategoryAsync(classCategoryId);
        var course = new Course
        {
            Name = name.Trim(),
            TermId = termId,
            ClassCategoryId = classCategoryId,
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

    public async Task<CourseUpdateResult> UpdateAsync(
    int id,
    string name,
    long termId,
    int classCategoryId,
    DateOnly startDate,
    DateOnly endDate,
    byte[] rowVersion)
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
        await ValidateTermAsync(termId, startDate, endDate);
        await ValidateClassCategoryAsync(classCategoryId);
        _db.Entry(course).Property(course => course.RowVersion).OriginalValue = rowVersion;

        course.Name = name.Trim();
        course.TermId = termId;
        course.ClassCategoryId = classCategoryId;
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

    public async Task<List<ClassCategory>> GetClassCategoryOptionsAsync()
    {
        return await _db.ClassCategories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.ClassCategoryId)
            .ToListAsync();
    }

    private async Task ValidateClassCategoryAsync(int classCategoryId)
    {
        var categoryExists = await _db.ClassCategories
            .AnyAsync(category =>
                category.ClassCategoryId == classCategoryId &&
                category.IsActive);

        if (!categoryExists)
        {
            throw new ArgumentException(
                "所選課程分類不存在或已停用，請重新選擇。");
        }
    }
}