using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class CourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db)
    {
        _db = db;
    }

    private async Task ValidateTermAsync(long termId)
    {
        var term = await _db.Terms.AsNoTracking().SingleOrDefaultAsync(item => item.TermId == termId);
        if (term is null)
        {
            throw new ArgumentException("所選學期不存在，請重新選擇。");
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
        .OrderByDescending(course => course.Term.StartDate)
        .ThenBy(course => course.CourseId)
        .ToListAsync();
    }

    public async Task CreateAsync(string name, long termId, int classCategoryId, long defaultTeacherId,
        long defaultLocationId, decimal termPrice, CourseSettings settings)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            throw new ArgumentException("課程名稱必填且最多 150 個字元。");
        }

        await ValidateTermAsync(termId);
        await ValidateClassCategoryAsync(classCategoryId);
        await ValidateDefaultTeacherAsync(defaultTeacherId);
        await ValidateDefaultLocationAsync(defaultLocationId);
        ValidateTermPrice(termPrice);
        ValidateSettings(settings);
        var course = new Course
        {
            Name = name.Trim(),
            TermId = termId,
            ClassCategoryId = classCategoryId,
            DefaultTeacherId = defaultTeacherId,
            DefaultLocationId = defaultLocationId,
            TermPrice = termPrice
        };

        ApplySettings(course, settings);
        _db.Courses.Add(course);

        await _db.SaveChangesAsync();
    }

    public async Task<Course?> GetByIdAsync(long courseId)
    {
        return await _db.Courses.AsNoTracking().SingleOrDefaultAsync(course => course.CourseId == courseId);
    }

    public async Task<CourseUpdateResult> UpdateAsync(long courseId, string name, long termId,
        int classCategoryId, long defaultTeacherId, long defaultLocationId, decimal termPrice,
        byte[] rowVersion, CourseSettings settings)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            throw new ArgumentException("課程名稱必填且最多 150 個字元。");
        }
        if (rowVersion is null || rowVersion.Length != 8)
        {
            throw new ArgumentException("資料版本無效。");
        }

        var course = await _db.Courses.SingleOrDefaultAsync(course => course.CourseId == courseId);
        if (course is null)
        {
            return CourseUpdateResult.NotFound;
        }
        if (!course.RowVersion.SequenceEqual(rowVersion))
        {
            return CourseUpdateResult.Conflict;
        }

        await ValidateTermAsync(termId);
        await ValidateClassCategoryAsync(classCategoryId);
        await ValidateDefaultTeacherAsync(defaultTeacherId);
        await ValidateDefaultLocationAsync(defaultLocationId);
        ValidateTermPrice(termPrice);
        ValidateSettings(settings);
        _db.Entry(course).Property(course => course.RowVersion).OriginalValue = rowVersion;

        course.Name = name.Trim();
        course.TermId = termId;
        course.ClassCategoryId = classCategoryId;
        course.DefaultTeacherId = defaultTeacherId;
        course.DefaultLocationId = defaultLocationId;
        course.TermPrice = termPrice;
        ApplySettings(course, settings);

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

    private static void ValidateSettings(CourseSettings settings)
    {
        if (settings.DefaultCapacity <= 0)
            throw new ArgumentException("預設名額必須大於 0。");
    }

    private static void ValidateTermPrice(decimal termPrice)
    {
        if (termPrice < 0 || termPrice > 99999999.99m ||
            decimal.Round(termPrice, 2) != termPrice)
            throw new ArgumentException("期課價格須介於 0～99999999.99，且最多兩位小數。");
    }

    private static void ApplySettings(Course course, CourseSettings settings)
    {
        course.DefaultCapacity = settings.DefaultCapacity;
        course.IsPubliclyVisible = settings.IsPubliclyVisible;
        course.AllowsFlexibleBooking = settings.AllowsFlexibleBooking;
        course.AllowsMakeup = settings.AllowsMakeup;
        course.IsActive = settings.IsActive;
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

    public async Task<List<Teacher>> GetTeacherOptionsAsync()
    {
        return await _db.Teachers
            .AsNoTracking()
            .Where(teacher => teacher.IsActive)
            .Include(teacher => teacher.User)
            .OrderBy(teacher => teacher.User.DisplayName)
            .ThenBy(teacher => teacher.TeacherId)
            .ToListAsync();
    }

    private async Task ValidateDefaultTeacherAsync(long defaultTeacherId)
    {
        var teacherExists = await _db.Teachers.AnyAsync(teacher => teacher.TeacherId == defaultTeacherId && teacher.IsActive);
        if (!teacherExists)
        {
            throw new ArgumentException( "所選預設老師不存在或已停用，請重新選擇。");
        }
    }

    public async Task<List<StudioLocation>> GetLocationOptionsAsync()
    {
        return await _db.StudioLocations
            .AsNoTracking()
            .Where(location => location.IsActive)
            .OrderBy(location => location.Name)
            .ThenBy(location => location.StudioLocationId)
            .ToListAsync();
    }

    private async Task ValidateDefaultLocationAsync(long defaultLocationId)
    {
        var locationExists = await _db.StudioLocations
            .AnyAsync(location =>
                location.StudioLocationId == defaultLocationId &&
                location.IsActive);

        if (!locationExists)
        {
            throw new ArgumentException(
                "所選預設地點不存在或已停用，請重新選擇。");
        }
    }
}
