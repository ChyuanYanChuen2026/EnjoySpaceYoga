using System.Security.Cryptography;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

public static class DbSeeder
{
    private const string TermName = "2026 秋季";
    private const string MiaAccount = "teacher-1";
    private const string CeliaAccount = "teacher-celia";
    private const string StudioName = "Enjoy Sport Space 享運動空間";
    private const string StudioAddress = "104臺北市中山區撫順街16號2樓";
    private static readonly DateOnly TermStartDate = new(2026, 9, 1);
    private static readonly DateOnly TermEndDate = new(2026, 10, 31);
    private static readonly TimeSpan TaiwanOffset = TimeSpan.FromHours(8);

    public static async Task<ScheduleSyncResult> SeedAsync(AppDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var term = await EnsureTermAsync(db);
        // Never let startup schedule synchronization rewrite a released or purchased timetable.
        if (term.Status != "Draft"
            || await db.EnrollmentApplications.AnyAsync(x => x.Course.TermId == term.TermId)
            || await db.CourseEnrollments.AnyAsync(x => x.Course.TermId == term.TermId))
        {
            Console.WriteLine("秋季課表已離開草稿或已有報名紀錄，略過自動課表同步。");
            return new ScheduleSyncResult(false, false, 0, 0, 0, 0, 0);
        }
        var location = await EnsureStudioLocationAsync(db);
        var (mia, miaCreated) = await EnsureTeacherAsync(
            db,
            MiaAccount,
            "Mia老師",
            note: null);
        var (celia, celiaCreated) = await EnsureTeacherAsync(
            db,
            CeliaAccount,
            "Celia老師",
            "2026 秋季正式課表老師；登入帳號尚未啟用。");

        var teachers = new Dictionary<string, Teacher>(StringComparer.OrdinalIgnoreCase)
        {
            [MiaAccount] = mia,
            [CeliaAccount] = celia
        };

        var categories = await db.ClassCategories
            .Where(item => item.IsActive &&
                           (item.Code == "Corporate" || item.Code == "General"))
            .ToDictionaryAsync(item => item.Code, StringComparer.OrdinalIgnoreCase);

        if (!categories.ContainsKey("Corporate") || !categories.ContainsKey("General"))
        {
            throw new InvalidOperationException("找不到啟用中的一般班或企業班分類，停止建立課表。");
        }

        var specifications = BuildSpecifications();
        var courses = new Dictionary<CourseSpecification, Course>();
        var coursesCreated = 0;
        var coursesUpdated = 0;

        foreach (var specification in specifications)
        {
            var matches = await db.Courses
                .Where(item => item.TermId == term.TermId &&
                               specification.CourseNames.Contains(item.Name))
                .ToListAsync();

            if (matches.Count > 1)
            {
                throw new InvalidOperationException(
                    $"課程「{specification.DisplayName}」找到 {matches.Count} 筆，請先處理重複課程。");
            }

            var category = categories[specification.CategoryCode];
            var defaultTeacher = teachers[specification.DefaultTeacherAccount];
            Course course;

            if (matches.Count == 0)
            {
                course = new Course
                {
                    Name = specification.DisplayName,
                    TermId = term.TermId,
                    ClassCategoryId = category.ClassCategoryId,
                    DefaultTeacherId = defaultTeacher.TeacherId,
                    DefaultLocationId = location.StudioLocationId,
                    DefaultCapacity = 10,
                    TermPrice = specification.TermPrice,
                    IsPubliclyVisible = true,
                    AllowsFlexibleBooking = true,
                    AllowsMakeup = true,
                    IsActive = true
                };
                db.Courses.Add(course);
                coursesCreated++;
            }
            else
            {
                course = matches[0];
                var changed =
                    course.ClassCategoryId != category.ClassCategoryId ||
                    course.DefaultTeacherId != defaultTeacher.TeacherId ||
                    course.DefaultLocationId != location.StudioLocationId ||
                    course.DefaultCapacity != 10 ||
                    course.TermPrice != specification.TermPrice ||
                    !course.IsActive;

                course.ClassCategoryId = category.ClassCategoryId;
                course.DefaultTeacherId = defaultTeacher.TeacherId;
                course.DefaultLocationId = location.StudioLocationId;
                course.DefaultCapacity = 10;
                course.TermPrice = specification.TermPrice;
                course.IsActive = true;

                if (changed)
                {
                    coursesUpdated++;
                }
            }

            courses.Add(specification, course);
        }

        await db.SaveChangesAsync();

        var courseIds = courses.Values.Select(item => item.CourseId).ToArray();
        var termStart = At(2026, 9, 1, 0, 0);
        var termEndExclusive = At(2026, 11, 1, 0, 0);
        var existingSessions = await db.ClassSessions
            .Where(item => courseIds.Contains(item.CourseId) &&
                           item.StartsAt >= termStart &&
                           item.StartsAt < termEndExclusive)
            .OrderBy(item => item.Id)
            .ToListAsync();

        var retainedIds = new HashSet<int>();
        var removedIds = new HashSet<int>();
        var sessionsCreated = 0;
        var sessionsUpdated = 0;
        var sessionsRemoved = 0;

        foreach (var (specification, course) in courses)
        {
            foreach (var expected in specification.Sessions)
            {
                var matches = existingSessions
                    .Where(item => item.CourseId == course.CourseId &&
                                   item.StartsAt == expected.StartsAt)
                    .ToList();
                var expectedTeacher = teachers[expected.TeacherAccount];

                if (matches.Count == 0)
                {
                    db.ClassSessions.Add(new ClassSession
                    {
                        CourseId = course.CourseId,
                        TeacherId = expectedTeacher.TeacherId,
                        StudioLocationId = location.StudioLocationId,
                        StartsAt = expected.StartsAt,
                        EndsAt = expected.EndsAt,
                        Capacity = course.DefaultCapacity,
                        CapacityOverride = null
                    });
                    sessionsCreated++;
                    continue;
                }

                var session = matches[0];
                retainedIds.Add(session.Id);
                var changed =
                    session.TeacherId != expectedTeacher.TeacherId ||
                    session.StudioLocationId != location.StudioLocationId ||
                    session.EndsAt != expected.EndsAt ||
                    session.Capacity != course.DefaultCapacity ||
                    session.CapacityOverride is not null;

                session.TeacherId = expectedTeacher.TeacherId;
                session.StudioLocationId = location.StudioLocationId;
                session.EndsAt = expected.EndsAt;
                session.Capacity = course.DefaultCapacity;
                session.CapacityOverride = null;

                if (changed)
                {
                    sessionsUpdated++;
                }

                foreach (var duplicate in matches.Skip(1))
                {
                    db.ClassSessions.Remove(duplicate);
                    removedIds.Add(duplicate.Id);
                    sessionsRemoved++;
                }
            }
        }

        foreach (var obsolete in existingSessions.Where(item =>
                     !retainedIds.Contains(item.Id) && !removedIds.Contains(item.Id)))
        {
            db.ClassSessions.Remove(obsolete);
            sessionsRemoved++;
        }

        await db.SaveChangesAsync();
        await EnjoySpace.Web.Services.ScheduleVersion.TouchDraftAsync(db, term.TermId);
        await transaction.CommitAsync();

        return new ScheduleSyncResult(
            miaCreated,
            celiaCreated,
            coursesCreated,
            coursesUpdated,
            sessionsCreated,
            sessionsUpdated,
            sessionsRemoved);
    }

    private static async Task<Term> EnsureTermAsync(AppDbContext db)
    {
        var term = await db.Terms.SingleOrDefaultAsync(item => item.Name == TermName);
        if (term is null)
        {
            term = new Term
            {
                Name = TermName,
                StartDate = TermStartDate,
                EndDate = TermEndDate,
                Status = "Draft"
            };
            db.Terms.Add(term);
            await db.SaveChangesAsync();
            return term;
        }

        if (term.StartDate != TermStartDate || term.EndDate != TermEndDate)
        {
            throw new InvalidOperationException($"學期「{TermName}」日期與正式課表不符，停止同步。");
        }

        return term;
    }

    private static async Task<StudioLocation> EnsureStudioLocationAsync(AppDbContext db)
    {
        var locations = await db.StudioLocations
            .Where(item => item.Name == StudioName || item.Address == StudioAddress)
            .ToListAsync();

        if (locations.Count > 1)
        {
            throw new InvalidOperationException("正式教室找到多筆資料，請先處理重複地點。");
        }

        var location = locations.SingleOrDefault();
        if (location is null)
        {
            location = new StudioLocation
            {
                Name = StudioName,
                Address = StudioAddress,
                Latitude = 25.063488m,
                Longitude = 121.517764m,
                CheckInRadiusMeters = 500,
                IsActive = true
            };
            db.StudioLocations.Add(location);
        }
        else
        {
            location.Name = StudioName;
            location.Address = StudioAddress;
            location.Latitude = 25.063488m;
            location.Longitude = 121.517764m;
            location.CheckInRadiusMeters = 500;
            location.IsActive = true;
        }

        await db.SaveChangesAsync();
        return location;
    }

    private static async Task<(Teacher Teacher, bool Created)> EnsureTeacherAsync(
        AppDbContext db,
        string account,
        string displayName,
        string? note)
    {
        var teacher = await db.Teachers
            .Include(item => item.User)
            .SingleOrDefaultAsync(item => item.User.Account == account);
        var created = false;

        if (teacher is null)
        {
            var user = await db.Users.SingleOrDefaultAsync(item => item.Account == account);
            if (user is null)
            {
                user = new User
                {
                    Account = account,
                    DisplayName = displayName,
                    IsActive = false
                };
                var unavailablePassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                user.PasswordHash = new PasswordHasher<User>().HashPassword(user, unavailablePassword);
                db.Users.Add(user);
            }

            teacher = new Teacher
            {
                User = user,
                IsActive = true,
                Note = note
            };
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();
            created = true;
        }
        else
        {
            teacher.IsActive = true;
            if (note is not null && string.IsNullOrWhiteSpace(teacher.Note))
            {
                teacher.Note = note;
            }
        }

        var teacherRole = await db.Roles.SingleAsync(item =>
            item.RoleCode == "Teacher" && item.IsActive);
        var hasTeacherRole = await db.UserRoles.AnyAsync(item =>
            item.UserId == teacher.UserId && item.RoleId == teacherRole.RoleId);
        if (!hasTeacherRole)
        {
            db.UserRoles.Add(new UserRole
            {
                UserId = teacher.UserId,
                RoleId = teacherRole.RoleId,
                AssignedByUserId = null
            });
        }

        await db.SaveChangesAsync();
        return (teacher, created);
    }

    private static IReadOnlyList<CourseSpecification> BuildSpecifications()
    {
        return
        [
            new(
                ["週一晚間瑜珈班"],
                "Corporate",
                MiaAccount,
                2460m,
                Sessions([7, 14, 21], 9, 18, 40, 80, MiaAccount)
                    .Concat(Sessions([5, 12, 19], 10, 18, 40, 80, MiaAccount)).ToArray()),
            new(
                ["週三晚間瑜珈班"],
                "General",
                MiaAccount,
                3690m,
                Sessions([2, 9, 16, 23, 30], 9, 20, 0, 80, MiaAccount)
                    .Concat(Sessions([7, 14, 21, 28], 10, 20, 0, 80, MiaAccount)).ToArray()),
            new(
                ["週四晚間瑜珈班"],
                "General",
                MiaAccount,
                3690m,
                Sessions([3, 10, 17, 24], 9, 19, 40, 80, MiaAccount)
                    .Concat(Sessions([1, 8, 15, 22, 29], 10, 19, 40, 80, MiaAccount)).ToArray()),
            new(
                ["週六TRX核心懸吊班", "週六白天TRX班"],
                "General",
                CeliaAccount,
                3280m,
                Sessions([5, 12, 19], 9, 9, 0, 60, CeliaAccount)
                    .Concat(Sessions([3, 17, 24, 31], 10, 9, 0, 60, CeliaAccount))
                    .Append(new SessionSpecification(
                        At(2026, 10, 10, 9, 0),
                        At(2026, 10, 10, 10, 0),
                        MiaAccount))
                    .OrderBy(item => item.StartsAt)
                    .ToArray()),
            new(
                ["週六白天瑜珈班", "週六上午瑜珈班"],
                "General",
                MiaAccount,
                3280m,
                Sessions([5, 12, 19], 9, 10, 30, 80, MiaAccount)
                    .Concat(Sessions([3, 10, 17, 24, 31], 10, 10, 30, 80, MiaAccount)).ToArray())
        ];
    }

    private static IEnumerable<SessionSpecification> Sessions(
        int[] days,
        int month,
        int hour,
        int minute,
        int durationMinutes,
        string teacherAccount)
    {
        return days.Select(day =>
        {
            var startsAt = At(2026, month, day, hour, minute);
            return new SessionSpecification(startsAt, startsAt.AddMinutes(durationMinutes), teacherAccount);
        });
    }

    private static DateTimeOffset At(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, TaiwanOffset);

    private sealed record CourseSpecification(
        string[] CourseNames,
        string CategoryCode,
        string DefaultTeacherAccount,
        decimal TermPrice,
        IReadOnlyList<SessionSpecification> Sessions)
    {
        public string DisplayName => CourseNames[0];
    }

    private sealed record SessionSpecification(
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        string TeacherAccount);
}

public sealed record ScheduleSyncResult(
    bool MiaCreated,
    bool CeliaCreated,
    int CoursesCreated,
    int CoursesUpdated,
    int SessionsCreated,
    int SessionsUpdated,
    int SessionsRemoved);
