using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class TermScheduleService(AppDbContext db, EnrollmentWorkflowService workflow)
{
    public Task<List<TermCourseScheduleItem>> GetSummaryAsync(long termId) => db.Courses.AsNoTracking()
        .Where(x => x.TermId == termId).OrderBy(x => x.Name).ThenBy(x => x.CourseId)
        .Select(x => new TermCourseScheduleItem
        {
            CourseId = x.CourseId, Name = x.Name, IsActive = x.IsActive,
            SessionCount = db.ClassSessions.Count(s => s.CourseId == x.CourseId),
            FirstSessionAt = db.ClassSessions.Where(s => s.CourseId == x.CourseId).Min(s => (DateTimeOffset?)s.StartsAt),
            LastSessionAt = db.ClassSessions.Where(s => s.CourseId == x.CourseId).Max(s => (DateTimeOffset?)s.EndsAt)
        }).ToListAsync();

    public async Task UpdateAsync(TermEditViewModel model, long actorId)
    {
        await workflow.RequireAdminAsync(actorId);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var term = await db.Terms.SingleOrDefaultAsync(x => x.TermId == model.TermId)
            ?? throw new InvalidOperationException("找不到學期。");
        if (model.RowVersion is null || !term.RowVersion.SequenceEqual(model.RowVersion))
            throw new InvalidOperationException("學期資料已變更，請重新開啟編輯頁。");
        if (!TermStatusOptions.CanChangeTo(term.Status, model.Status))
            throw new InvalidOperationException($"不允許從「{TermStatusOptions.GetText(term.Status)}」變更為「{TermStatusOptions.GetText(model.Status)}」。");
        if (term.Status == TermStatusOptions.Closed && model.Status == TermStatusOptions.Draft)
        {
            if (await db.EnrollmentApplications.AnyAsync(x => x.Course.TermId == term.TermId)
                || await db.CourseEnrollments.AnyAsync(x => x.Course.TermId == term.TermId))
                throw new InvalidOperationException("此學期已有報名紀錄，不能退回草稿或直接增刪場次。");
            term.ScheduleConfirmedAtUtc = null;
            term.ScheduleConfirmedByUserId = null;
        }
        if (term.Status != TermStatusOptions.Draft && (term.StartDate != model.StartDate || term.EndDate != model.EndDate))
            throw new InvalidOperationException("學期離開草稿後不能變更上課期間，請保留原日期。");

        var start = new DateTimeOffset(model.StartDate!.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var end = new DateTimeOffset(model.EndDate!.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.FromHours(8));
        if (await db.ClassSessions.AnyAsync(x => x.Course.TermId == term.TermId && (x.StartsAt < start || x.EndsAt > end)))
            throw new InvalidOperationException("現有場次超出學期期間，請先核對課表或學期日期。");

        if (model.Status == TermStatusOptions.Open)
        {
            var courses = (await GetSummaryAsync(term.TermId)).Where(x => x.IsActive).ToList();
            if (courses.Count == 0)
                throw new InvalidOperationException("請先建立啟用中的課程及完整場次，再開放報名。");
            var incomplete = courses.Where(x => x.SessionCount is < 1 or > short.MaxValue).Select(x => x.Name).ToList();
            if (incomplete.Count > 0)
                throw new InvalidOperationException($"以下課程尚無有效場次：{string.Join("、", incomplete)}。");
            if (term.Status != TermStatusOptions.Open || term.ScheduleConfirmedAtUtc is null)
            {
                if (!model.ConfirmScheduleComplete)
                    throw new InvalidOperationException("請核對整個學期課表，並勾選排課完成確認後再開放報名。");
                term.ScheduleConfirmedAtUtc = DateTime.UtcNow;
                term.ScheduleConfirmedByUserId = actorId;
            }
        }

        db.Entry(term).Property(x => x.RowVersion).OriginalValue = model.RowVersion;
        term.Name = model.Name.Trim();
        term.StartDate = model.StartDate.Value;
        term.EndDate = model.EndDate.Value;
        term.RegistrationStartAtUtc = ToUtc(model.RegistrationStartAtTaiwan);
        term.RegistrationEndAtUtc = ToUtc(model.RegistrationEndAtTaiwan);
        term.Status = model.Status;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static DateTime? ToUtc(DateTime? time) => time.HasValue
        ? new DateTimeOffset(DateTime.SpecifyKind(time.Value, DateTimeKind.Unspecified), TimeSpan.FromHours(8)).UtcDateTime
        : null;
}
