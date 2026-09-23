using EnjoySpace.Web.Models.Enums;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

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

        var teacher = await db.Teachers
            .OrderBy(teacher => teacher.Id)
            .FirstOrDefaultAsync(teacher =>
                teacher.Name == teacherName);

        if (teacher is null)
        {
            teacher = new Teacher
            {
                Name = teacherName
            };

            db.Teachers.Add(teacher);
        }

        const string locationName = "米亞工作室多瑜珈教室";

        var location = await db.Locations
            .OrderBy(location => location.Id)
            .FirstOrDefaultAsync(location =>
                location.Name == locationName);

        if (location is null)
        {
            location = new Location
            {
                Name = locationName
            };

            db.Locations.Add(location);
        }

        await db.SaveChangesAsync();

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
                TeacherId = teacher.Id,
                LocationId = location.Id,
                StartsAt = startsAt,
                EndsAt = endsAt,
                Capacity = 10,
                CapacityOverride = null
            };

            db.ClassSessions.Add(session);

            await db.SaveChangesAsync();
        }

        const string studentEmail = "demo.student@example.com";

        var student = await db.Students
            .OrderBy(student => student.Id)
            .FirstOrDefaultAsync(student =>
                student.Email == studentEmail);

        if (student is null)
        {
            student = new Student
            {
                Name = "測試學生",
                Email = studentEmail
            };

            db.Students.Add(student);

            await db.SaveChangesAsync();
        }

        var applicationExists = await db.EnrollmentApplications
            .AnyAsync(application =>
                application.StudentId == student.Id &&
                application.CourseId == course.Id);

        if (!applicationExists)
        {
            var application = new EnrollmentApplication
            {
                StudentId = student.Id,
                CourseId = course.Id,
                Status = EnrollmentApplicationStatus.Pending,
                SubmittedAt = DateTimeOffset.UtcNow
            };

            db.EnrollmentApplications.Add(application);

            await db.SaveChangesAsync();
        }

        await transaction.CommitAsync();
    }
}