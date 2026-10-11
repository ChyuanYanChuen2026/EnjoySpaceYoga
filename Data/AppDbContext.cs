using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<PortalTrialRequest> PortalTrialRequests => Set<PortalTrialRequest>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<EnrollmentApplication> EnrollmentApplications => Set<EnrollmentApplication>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<ClassCategory> ClassCategories => Set<ClassCategory>();
    public DbSet<StudioLocation> StudioLocations => Set<StudioLocation>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TrialGuest> TrialGuests => Set<TrialGuest>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<ClassSessionParticipant> ClassSessionParticipants => Set<ClassSessionParticipant>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<MakeupEntitlement> MakeupEntitlements => Set<MakeupEntitlement>();
    public DbSet<MakeupRequest> MakeupRequests => Set<MakeupRequest>();
    public DbSet<LessonPlan> LessonPlans => Set<LessonPlan>();
    public DbSet<LessonPass> LessonPasses => Set<LessonPass>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PortalTrialRequest>().HasIndex(x => x.SubmissionKey).IsUnique();
        modelBuilder.Entity<PortalTrialRequest>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<PortalTrialRequest>().HasOne(x => x.TrialGuest).WithMany().HasForeignKey(x => x.TrialGuestId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<PortalTrialRequest>().HasOne(x => x.ClassSession).WithMany().HasForeignKey(x => x.ClassSessionId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<PortalTrialRequest>().HasOne(x => x.LessonPlan).WithMany().HasForeignKey(x => x.LessonPlanId).OnDelete(DeleteBehavior.NoAction);
        EnrollmentModelConfiguration.Configure(modelBuilder);
        AttendanceModelConfiguration.Configure(modelBuilder);
        LeaveMakeupModelConfiguration.Configure(modelBuilder);
        LessonPlanModelConfiguration.Configure(modelBuilder);

        modelBuilder.Entity<Course>().Property(course => course.DefaultCapacity)
            .HasDefaultValue((short)10, "DF_Courses_DefaultCapacity");
        modelBuilder.Entity<Course>().Property(course => course.IsPubliclyVisible)
            .HasDefaultValue(true, "DF_Courses_IsPubliclyVisible");
        modelBuilder.Entity<Course>().Property(course => course.AllowsFlexibleBooking)
            .HasDefaultValue(true, "DF_Courses_AllowsFlexibleBooking");
        modelBuilder.Entity<Course>().Property(course => course.AllowsMakeup)
            .HasDefaultValue(true, "DF_Courses_AllowsMakeup");
        modelBuilder.Entity<Course>().Property(course => course.IsActive)
            .HasDefaultValue(true, "DF_Courses_IsActive");
        modelBuilder.Entity<Course>().Property(course => course.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()", "DF_Courses_CreatedAtUtc");

        modelBuilder.Entity<Course>()
            .Property(course => course.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Course>()
            .HasOne(course => course.Term)
            .WithMany()
            .HasForeignKey(course => course.TermId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Courses_Terms");

        modelBuilder.Entity<Course>()
            .HasOne(course => course.ClassCategory)
            .WithMany()
            .HasForeignKey(course => course.ClassCategoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Courses_ClassCategories");

        modelBuilder.Entity<Course>()
            .HasOne(course => course.DefaultTeacher)
            .WithMany()
            .HasForeignKey(course => course.DefaultTeacherId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Courses_DefaultTeacher");

        modelBuilder.Entity<Course>()
            .HasOne(course => course.DefaultLocation)
            .WithMany()
            .HasForeignKey(course => course.DefaultLocationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Courses_DefaultLocation");

        modelBuilder.Entity<ClassSession>()
            .HasOne(session => session.Course)
            .WithMany()
            .HasForeignKey(session => session.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClassSession>()
            .HasOne(session => session.Teacher)
            .WithMany()
            .HasForeignKey(session => session.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClassSession>()
            .Property(session => session.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<ClassSession>()
            .HasOne(session => session.StudioLocation)
            .WithMany()
            .HasForeignKey(session => session.StudioLocationId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ClassSessions_Locations");

        modelBuilder.Entity<Teacher>()
            .HasOne(teacher => teacher.User)
            .WithOne()
            .HasForeignKey<Teacher>(teacher => teacher.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Teachers_Users");

        modelBuilder.Entity<Teacher>()
            .HasAlternateKey(teacher => teacher.UserId)
            .HasName("UQ_Teachers_UserId");

        modelBuilder.Entity<Teacher>()
            .HasIndex(teacher => teacher.TeacherNo)
            .IsUnique()
            .HasFilter("[TeacherNo] IS NOT NULL")
            .HasDatabaseName("UX_Teachers_TeacherNo");

        modelBuilder.Entity<Teacher>()
            .Property(teacher => teacher.IsActive)
            .HasDefaultValue(true, "DF_Teachers_IsActive");

        modelBuilder.Entity<Teacher>()
            .Property(teacher => teacher.CreatedAtUtc)
            .HasDefaultValueSql( "SYSUTCDATETIME()", "DF_Teachers_CreatedAtUtc");

        modelBuilder.Entity<Student>()
            .HasOne(student => student.User)
            .WithOne()
            .HasForeignKey<Student>(student => student.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Students_Users");

        modelBuilder.Entity<Student>()
            .HasAlternateKey(student => student.UserId)
            .HasName("UQ_Students_UserId");

        modelBuilder.Entity<Student>()
            .HasIndex(student => student.StudentNo)
            .IsUnique()
            .HasFilter("[StudentNo] IS NOT NULL")
            .HasDatabaseName("UX_Students_StudentNo");

        modelBuilder.Entity<Student>()
            .Property(student => student.IsActive)
            .HasDefaultValue(true, "DF_Students_IsActive");

        modelBuilder.Entity<Student>()
            .Property(student => student.CreatedAtUtc)
            .HasDefaultValueSql( "SYSUTCDATETIME()",  "DF_Students_CreatedAtUtc");

        modelBuilder.Entity<EnrollmentApplication>()
            .HasOne(application => application.Student)
            .WithMany()
            .HasForeignKey(application => application.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EnrollmentApplication>()
            .HasOne(application => application.Course)
            .WithMany()
            .HasForeignKey(application => application.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EnrollmentApplication>()
            .Property(application => application.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<User>()
            .HasAlternateKey(user => user.Account)
            .HasName("UQ_Users_Account");

        modelBuilder.Entity<User>()
            .Property(user => user.IsActive)
            .HasDefaultValue(true, "DF_Users_IsActive");

        modelBuilder.Entity<User>()
            .Property(user => user.CreatedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()", "DF_Users_CreatedAtUtc");

        modelBuilder.Entity<User>()
            .Property(user => user.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Role>()
            .HasAlternateKey(role => role.RoleCode)
            .HasName("UQ_Roles_RoleCode");

        modelBuilder.Entity<Role>()
            .Property(role => role.IsActive)
            .HasDefaultValue(true, "DF_Roles_IsActive");

        modelBuilder.Entity<UserRole>()
            .HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            })
            .HasName("PK_UserRoles");

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.User)
            .WithMany()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserRoles_Users");

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.Role)
            .WithMany()
            .HasForeignKey(userRole => userRole.RoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserRoles_Roles");

        modelBuilder.Entity<UserRole>()
            .HasOne(userRole => userRole.AssignedByUser)
            .WithMany()
            .HasForeignKey(userRole => userRole.AssignedByUserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_UserRoles_AssignedBy");

        modelBuilder.Entity<UserRole>()
            .Property(userRole => userRole.AssignedAtUtc)
            .HasDefaultValueSql("SYSUTCDATETIME()","DF_UserRoles_AssignedAtUtc");

        modelBuilder.Entity<Term>()
            .Property(term => term.Status)
            .HasDefaultValue("Draft", "DF_Terms_Status");

        modelBuilder.Entity<Term>()
            .Property(term => term.CreatedAtUtc)
            .HasDefaultValueSql( "SYSUTCDATETIME()", "DF_Terms_CreatedAtUtc");

        modelBuilder.Entity<Term>()
            .Property(term => term.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<ClassCategory>()
            .HasAlternateKey(category => category.Code)
            .HasName("UQ_ClassCategories_Code");

        modelBuilder.Entity<ClassCategory>()
            .Property(category => category.IsActive)
            .HasDefaultValue(true, "DF_ClassCategories_IsActive");

        modelBuilder.Entity<StudioLocation>()
            .Property(location => location.CheckInRadiusMeters)
            .HasDefaultValue(500, "DF_StudioLocations_Radius");

        modelBuilder.Entity<StudioLocation>()
            .Property(location => location.IsActive)
            .HasDefaultValue(true, "DF_StudioLocations_IsActive");

        modelBuilder.Entity<StudioLocation>()
            .Property(location => location.CreatedAtUtc)
            .HasDefaultValueSql( "SYSUTCDATETIME()", "DF_StudioLocations_CreatedAtUtc");

    }
}
