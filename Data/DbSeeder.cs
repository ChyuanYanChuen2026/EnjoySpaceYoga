using EnjoySpace.Web.Models.Enums;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace EnjoySpace.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        const string courseName = "2026秋季週一晚間瑜珈班";
        var startDate = new DateOnly(2026, 9, 7);
        var endDate = new DateOnly(2026, 10, 26);

        var course = await db.Courses
            .OrderBy(course => course.Id)
            .FirstOrDefaultAsync(course =>
                course.Name == courseName &&
                course.StartDate == startDate &&
                course.EndDate == endDate);

        if (course is null)
        {
            course = new Course
            {
                Name = courseName,
                StartDate = startDate,
                EndDate = endDate
            };

            db.Courses.Add(course);
        }

        const string teacherName = "Mia老師";
        const string seedAccount = "teacher-1";

        var teacher = await db.Teachers.SingleOrDefaultAsync(teacher => teacher.User.Account == seedAccount);

        if (teacher is null)
        {
            var seedAccountExists = await db.Users.AnyAsync(user => user.Account == seedAccount);
            if (seedAccountExists)
            {
                throw new InvalidOperationException(
                    $"帳號 {seedAccount} 已存在，請先確認老師對應。");
            }
            var seedTeacherRole = await db.Roles.SingleAsync(role =>role.RoleCode == "Teacher" && role.IsActive);
            var seedUser = new User
            {
                Account = seedAccount,
                DisplayName = teacherName,
                IsActive = false
            };
            var seedPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            seedUser.PasswordHash = new PasswordHasher<User>().HashPassword(seedUser, seedPassword);
            teacher = new Teacher
            {
                User = seedUser,
                IsActive = true
            };
            db.Teachers.Add(teacher);
            db.UserRoles.Add(new UserRole
            {
                User = seedUser,
                RoleId = seedTeacherRole.RoleId,
                AssignedByUserId = null
            });
        }

        await db.SaveChangesAsync();

        var termSeeds = new[]
        {
            new
            {
                Name = "2026 秋季",
                StartDate = new DateOnly(2026, 9, 1),
                EndDate = new DateOnly(2026, 10, 31),
                CourseNames = new[]
                {
                    "2026秋季週一晚間瑜珈班",
                    "2026秋季週四晚間瑜珈班"
                }
            },
            new
            {
                Name = "2026 冬季",
                StartDate = new DateOnly(2026, 11, 1),
                EndDate = new DateOnly(2026, 12, 31),
                CourseNames = new[]
                {
                    "2026冬季週三晚間瑜珈班"
                }
            }
        };

        foreach (var termSeed in termSeeds)
        {
            var term = await db.Terms
                .SingleOrDefaultAsync(item => item.Name == termSeed.Name);

            if (term is null)
            {
                term = new Term
                {
                    Name = termSeed.Name,
                    StartDate = termSeed.StartDate,
                    EndDate = termSeed.EndDate,
                    Status = "Draft"
                };

                db.Terms.Add(term);

                await db.SaveChangesAsync();
            }
            else if (term.StartDate != termSeed.StartDate ||
                     term.EndDate != termSeed.EndDate)
            {
                throw new InvalidOperationException(
                    $"學期「{termSeed.Name}」日期與預期不符，請先確認。");
            }

            foreach (var targetCourseName in termSeed.CourseNames)
            {
                var courseToLink = await db.Courses
                    .SingleOrDefaultAsync(item => item.Name == targetCourseName);

                if (courseToLink is null)
                {
                    throw new InvalidOperationException(
                        $"找不到課程「{targetCourseName}」，請先確認課程名稱。");
                }

                if (courseToLink.StartDate < term.StartDate ||
                    courseToLink.EndDate > term.EndDate)
                {
                    throw new InvalidOperationException(
                        $"課程「{targetCourseName}」的日期超出學期範圍。");
                }

                if (courseToLink.TermId.HasValue &&
                    courseToLink.TermId.Value != term.TermId)
                {
                    throw new InvalidOperationException(
                        $"課程「{targetCourseName}」已對應其他學期，停止修改。");
                }

                if (courseToLink.TermId is null)
                {
                    courseToLink.TermId = term.TermId;
                }
            }

            await db.SaveChangesAsync();
        }

        const string studioName = "Enjoy Sport Space 享運動空間";
        const string studioAddress = "104臺北市中山區撫順街16號2樓";

        var studioLocation = await db.StudioLocations
            .SingleOrDefaultAsync(location =>
                location.Name == studioName &&
                location.Address == studioAddress);

        if (studioLocation is null)
        {
            studioLocation = new StudioLocation
            {
                Name = studioName,
                Address = studioAddress,
                Latitude = 25.063488m,
                Longitude = 121.517764m,
                CheckInRadiusMeters = 500,
                IsActive = true
            };

            db.StudioLocations.Add(studioLocation);

            await db.SaveChangesAsync();
        }

        var startsAt = new DateTimeOffset(
            2026, 9, 28, 19, 0, 0, TimeSpan.FromHours(8));

        var endsAt = startsAt.AddHours(1);

        var sessionExists = await db.ClassSessions.AnyAsync(session =>
            session.CourseId == course.Id &&
            session.StartsAt == startsAt);

        if (!sessionExists)
        {
            var session = new ClassSession
            {
                CourseId = course.Id,
                TeacherId = teacher.TeacherId,
                StudioLocationId = studioLocation.StudioLocationId,
                StartsAt = startsAt,
                EndsAt = endsAt,
                Capacity = 10,
                CapacityOverride = null
            };

            db.ClassSessions.Add(session);

            await db.SaveChangesAsync();
        }

        const string studentEmail = "demo.student@example.com";
        const string seedStudentAccount = "student-1";

        var student = await db.Students
            .SingleOrDefaultAsync(student =>
                student.User.Account == seedStudentAccount);

        if (student is null)
        {
            var seedStudentAccountExists = await db.Users
                .AnyAsync(user => user.Account == seedStudentAccount);
            if (seedStudentAccountExists)
            {
                throw new InvalidOperationException(
                    $"帳號 {seedStudentAccount} 已存在，請先確認學生對應。");
            }
            var seedStudentRole = await db.Roles.SingleAsync(role => role.RoleCode == "Student" && role.IsActive);
            var seedStudentUser = new User
            {
                Account = seedStudentAccount,
                DisplayName = "測試學生",
                Email = studentEmail,
                IsActive = false
            };
            var seedStudentPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            seedStudentUser.PasswordHash = new PasswordHasher<User>().HashPassword(seedStudentUser, seedStudentPassword);
            student = new Student
            {
                User = seedStudentUser,
                IsActive = true
            };
            db.Students.Add(student);
            db.UserRoles.Add(new UserRole
            {
                User = seedStudentUser,
                RoleId = seedStudentRole.RoleId,
                AssignedByUserId = null
            });
            await db.SaveChangesAsync();
        }

        var applicationExists = await db.EnrollmentApplications.AnyAsync(application => application.StudentId == student.StudentId && application.CourseId == course.Id);
        if (!applicationExists)
        {
            var application = new EnrollmentApplication
            {
                StudentId = student.StudentId,
                CourseId = course.Id,
                Status = EnrollmentApplicationStatus.Pending,
                SubmittedAt = DateTimeOffset.UtcNow
            };

            db.EnrollmentApplications.Add(application);

            await db.SaveChangesAsync();
            await db.SaveChangesAsync();
        }

        await db.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}