using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.EntityFrameworkCore;

internal static class TableSortChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, Action<bool, string> assert)
    {
        await using var db = createDb();
        async Task Verify<T>(IQueryable<T> query, TableSort<T> definition, string name)
        {
            foreach (var option in definition.Options())
                await definition.Apply(query, option.Value).Take(2).ToListAsync();
            assert(true, "SQL executes every ascending/descending header for " + name);
        }
        await Verify(db.Terms, AdminTableSorts.Terms, "terms");
        await Verify(db.Courses, AdminTableSorts.Courses, "courses");
        await Verify(db.ClassSessions, AdminTableSorts.Sessions, "sessions");
        await Verify(db.Students, AdminTableSorts.Students, "students");
        await Verify(db.Teachers, AdminTableSorts.Teachers, "teachers");
        await Verify(db.LessonPlans, AdminTableSorts.Plans, "plans");
        await Verify(db.LessonPasses, AdminTableSorts.Passes, "passes");
        await Verify(db.TrialGuests, AdminTableSorts.Guests, "guests");
        await Verify(db.Payments, AdminTableSorts.Payments, "payments");
        await Verify(db.PortalTrialRequests, AdminTableSorts.PortalTrials, "portal trials");
        await Verify(db.EnrollmentApplications, AdminTableSorts.Applications, "applications");
        await Verify(db.CourseEnrollments, AdminTableSorts.Enrollments, "enrollments");
        await Verify(db.TeacherRequests, AdminTableSorts.TeacherRequests, "teacher requests");

        await using var transaction = await db.Database.BeginTransactionAsync();
        var plans = Enumerable.Range(1, 25).Select(i => new LessonPlan {
            PlanCode = "sort-check-" + i, PlanName = "排序驗證 " + i, Price = i % 3 == 0 ? 2 : i % 3 == 1 ? 100 : 10,
            SessionQuota = 1, ValidityUnit = "Days", ValidityValue = (short)i }).ToList();
        db.LessonPlans.AddRange(plans);
        await db.SaveChangesAsync();
        var query = db.LessonPlans.Where(x => x.PlanCode.StartsWith("sort-check-"));
        var first = await TrialLessonService.PageAsync(AdminTableSorts.Plans.Apply(query, "price_asc"), 1);
        var second = await TrialLessonService.PageAsync(AdminTableSorts.Plans.Apply(query, "price_asc"), 2);
        var expected = plans.OrderBy(x => x.Price).ThenBy(x => x.LessonPlanId).Select(x => x.LessonPlanId);
        assert(first.Items.Concat(second.Items).Select(x => x.LessonPlanId).SequenceEqual(expected),
            "numeric price sorting spans pages with stable ties and no omitted rows");
        var descending = await AdminTableSorts.Plans.Apply(query, "price_desc").ToListAsync();
        assert(descending.Select(x => x.Price).SequenceEqual(plans.OrderByDescending(x => x.Price).Select(x => x.Price)),
            "descending prices are numeric, not formatted text");
        var validity = await AdminTableSorts.Plans.Apply(query, "validity_asc").ToListAsync();
        assert(validity.Select(x => x.ValidityValue).SequenceEqual(plans.Select(x => x.ValidityValue)), "validity sorts numeric values within a unit");
        var fallback = await AdminTableSorts.Plans.Apply(query, "Price; DROP TABLE LessonPlans_desc").ToListAsync();
        assert(fallback.Select(x => x.LessonPlanId).SequenceEqual(plans.OrderByDescending(x => x.LessonPlanId).Select(x => x.LessonPlanId)),
            "unregistered sort falls back to default order");
        var catalog = new AdminCatalogService(db);
        var coursePage = await catalog.GetCoursesAsync(new() { Sort = "price_desc" });
        assert(coursePage.Controls.Filter.Sort == "price_desc", "catalog preserves header price sort during normalization");
        var termPage = await catalog.GetTermsAsync(new() { Sort = "registrationEnd_asc" });
        assert(termPage.Controls.Filter.Sort == "registrationEnd_asc", "catalog supports nullable registration date sort");
        await transaction.RollbackAsync();
    }
}
