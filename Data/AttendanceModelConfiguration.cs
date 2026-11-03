using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

internal static class AttendanceModelConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var participant = model.Entity<ClassSessionParticipant>();
        participant.HasOne(x => x.ClassSession).WithMany().HasForeignKey(x => x.ClassSessionId).OnDelete(DeleteBehavior.NoAction);
        participant.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.NoAction);
        participant.HasOne(x => x.CourseEnrollment).WithMany().HasForeignKey(x => x.CourseEnrollmentId).OnDelete(DeleteBehavior.NoAction);
        participant.HasOne(x => x.LessonPass).WithMany().HasForeignKey(x => x.LessonPassId).OnDelete(DeleteBehavior.NoAction);
        participant.HasOne(x => x.TrialRequest).WithMany().HasForeignKey(x => x.TrialRequestId).OnDelete(DeleteBehavior.NoAction);
        participant.HasIndex(x => x.TrialRequestId);
        participant.HasIndex(x => x.BookingKey).IsUnique().HasFilter("[BookingKey] IS NOT NULL");
        participant.ToTable("ClassSessionParticipants", "dbo", t => t.HasCheckConstraint("CK_ClassSessionParticipants_Source",
            "([ParticipationType]=N'Regular' AND [CourseEnrollmentId] IS NOT NULL AND [LessonPassId] IS NULL OR [ParticipationType]=N'Makeup' AND [MakeupRequestId] IS NOT NULL AND [LessonPassId] IS NULL OR [ParticipationType]=N'Trial' AND [CourseEnrollmentId] IS NULL AND [LessonPassId] IS NOT NULL AND [TrialRequestId] IS NOT NULL OR [ParticipationType]=N'Flexible' AND [CourseEnrollmentId] IS NULL AND [LessonPassId] IS NOT NULL AND [BookingKey] IS NOT NULL)"));
        participant.HasOne<User>().WithMany().HasForeignKey(x => x.AddedByUserId).OnDelete(DeleteBehavior.NoAction);
        participant.HasIndex(x => new { x.ClassSessionId, x.StudentId }).IsUnique()
            .HasFilter("[Status] = N'Active'").HasDatabaseName("UX_ClassSessionParticipants_ActiveStudent");
        participant.HasIndex(x => new { x.ClassSessionId, x.CourseEnrollmentId }).IsUnique()
            .HasFilter("[ParticipationType] = N'Regular'");
        participant.Property(x => x.AddedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        var attendance = model.Entity<AttendanceRecord>();
        attendance.HasOne(x => x.Participant).WithOne(x => x.Attendance)
            .HasForeignKey<AttendanceRecord>(x => x.SessionParticipantId).OnDelete(DeleteBehavior.NoAction);
        attendance.HasOne(x => x.CheckedInByUser).WithMany().HasForeignKey(x => x.CheckedInByUserId).OnDelete(DeleteBehavior.NoAction);
        attendance.HasIndex(x => x.Status);
        attendance.ToTable("AttendanceRecords", "dbo", t => t.HasCheckConstraint("CK_AttendanceRecords_Status",
            "[Status] IN (N'Scheduled', N'Present', N'Late', N'Absent')"));
    }
}
