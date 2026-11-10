using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

// Filter and sort before paging so each page reflects the complete matching result set.
public class AdminCatalogService(AppDbContext db)
{
    public async Task<CatalogListViewModel<TermListItemViewModel>> GetTermsAsync(CatalogFilter filter)
    {
        var controls = new CatalogListControls
        {
            Filter = filter,
            SearchLabel = "學期名稱",
            StatusOptions = TermStatusOptions.Items.Select(x => new SelectListItem(x.Text, x.Value)).ToList(),
            SortOptions = AdminTableSorts.Terms.Options()
        };
        filter.Sort = AdminTableSorts.Terms.Normalize(filter.Sort);
        Normalize(controls);
        var query = db.Terms.AsNoTracking().AsQueryable();
        if (filter.Search is { } search) query = query.Where(x => x.Name.Contains(search));
        if (filter.Status is { } status) query = query.Where(x => x.Status == status);
        var ordered = AdminTableSorts.Terms.Apply(query, filter.Sort);
        return await PageAsync(ordered.ThenBy(x => x.TermId).Select(x => new TermListItemViewModel
        {
            TermId = x.TermId, Name = x.Name, StartDate = x.StartDate, EndDate = x.EndDate,
            RegistrationStartAtUtc = x.RegistrationStartAtUtc, RegistrationEndAtUtc = x.RegistrationEndAtUtc,
            Status = x.Status
        }), controls);
    }

    public async Task<CatalogListViewModel<Course>> GetCoursesAsync(CatalogFilter filter)
    {
        var controls = new CatalogListControls
        {
            Filter = filter,
            SearchLabel = "課程名稱",
            TermOptions = await TermOptionsAsync(),
            StatusOptions = Options(("Active", "啟用"), ("Inactive", "停用")),
            SortOptions = AdminTableSorts.Courses.Options()
        };
        filter.Sort = AdminTableSorts.Courses.Normalize(filter.Sort);
        Normalize(controls);
        var query = db.Courses.AsNoTracking().Include(x => x.Term).AsQueryable();
        if (filter.Search is { } search) query = query.Where(x => x.Name.Contains(search));
        if (filter.TermId is { } termId) query = query.Where(x => x.TermId == termId);
        if (filter.Status is { } status) query = query.Where(x => x.IsActive == (status == "Active"));
        var ordered = AdminTableSorts.Courses.Apply(query, filter.Sort);
        return await PageAsync(ordered.ThenBy(x => x.CourseId), controls);
    }

    public async Task<CatalogListViewModel<ClassSessionListItem>> GetSessionsAsync(CatalogFilter filter)
    {
        var controls = new CatalogListControls
        {
            Filter = filter,
            SearchLabel = "課程／老師／地點",
            TermOptions = await TermOptionsAsync(),
            StatusOptions = Options(("Upcoming", "尚未開始"), ("Ongoing", "上課中"), ("Ended", "已結束")),
            SortOptions = AdminTableSorts.Sessions.Options()
        };
        filter.Sort = AdminTableSorts.Sessions.Normalize(filter.Sort);
        Normalize(controls);
        controls.CourseOptions = await db.Courses.AsNoTracking()
            .Where(x => !filter.TermId.HasValue || x.TermId == filter.TermId.Value)
            .OrderByDescending(x => x.Term.StartDate).ThenBy(x => x.Name).ThenBy(x => x.CourseId)
            .Select(x => new SelectListItem(x.Term.Name + " · " + x.Name, x.CourseId.ToString()))
            .ToListAsync();
        if (filter.CourseId.HasValue && !controls.CourseOptions.Any(x => x.Value == filter.CourseId.Value.ToString()))
            filter.CourseId = null;
        var query = db.ClassSessions.AsNoTracking().AsQueryable();
        if (filter.Search is { } search)
            query = query.Where(x => x.Course.Name.Contains(search) || x.Teacher.User.DisplayName.Contains(search)
                || x.StudioLocation.Name.Contains(search));
        if (filter.TermId is { } termId) query = query.Where(x => x.Course.TermId == termId);
        if (filter.CourseId is { } courseId) query = query.Where(x => x.CourseId == courseId);
        var now = DateTimeOffset.UtcNow;
        query = filter.Status switch
        {
            "Upcoming" => query.Where(x => x.StartsAt > now),
            "Ongoing" => query.Where(x => x.StartsAt <= now && x.EndsAt > now),
            "Ended" => query.Where(x => x.EndsAt <= now),
            _ => query
        };
        var ordered = AdminTableSorts.Sessions.Apply(query, filter.Sort);
        return await PageAsync(ordered.ThenBy(x => x.Id).Select(x => new ClassSessionListItem
        {
            Id = x.Id, CourseName = x.Course.Name, TeacherName = x.Teacher.User.DisplayName,
            LocationName = x.StudioLocation.Name, StartsAt = x.StartsAt, EndsAt = x.EndsAt,
            EffectiveCapacity = x.CapacityOverride ?? x.Capacity,
            CanDelete = !x.IsExtraSession && x.Course.Term.Status == TermStatusOptions.Draft
                && !db.EnrollmentApplications.Any(a => a.CourseId == x.CourseId || a.FirstSessionId == x.Id)
                && !db.CourseEnrollments.Any(e => e.CourseId == x.CourseId)
        }), controls);
    }

    private Task<List<SelectListItem>> TermOptionsAsync() => db.Terms.AsNoTracking()
        .OrderByDescending(x => x.StartDate).ThenBy(x => x.TermId)
        .Select(x => new SelectListItem(x.Name, x.TermId.ToString())).ToListAsync();

    private static List<SelectListItem> Options(params (string Value, string Text)[] items) =>
        items.Select(x => new SelectListItem(x.Text, x.Value)).ToList();

    private static void Normalize(CatalogListControls controls)
    {
        var filter = controls.Filter;
        filter.Search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
        if (!controls.StatusOptions.Any(x => x.Value == filter.Status)) filter.Status = null;
        if (!controls.SortOptions.Any(x => x.Value == filter.Sort)) filter.Sort = controls.SortOptions[0].Value;
        if (filter.TermId <= 0) filter.TermId = null;
        if (filter.CourseId <= 0) filter.CourseId = null;
    }

    private static async Task<CatalogListViewModel<T>> PageAsync<T>(IQueryable<T> query, CatalogListControls controls)
    {
        controls.TotalCount = await query.CountAsync();
        controls.Filter.Page = Math.Clamp(controls.Filter.Page, 1, controls.TotalPages);
        var items = await query.Skip((controls.Filter.Page - 1) * CatalogListControls.PageSize)
            .Take(CatalogListControls.PageSize).ToListAsync();
        return new CatalogListViewModel<T> { Items = items, Controls = controls };
    }
}
