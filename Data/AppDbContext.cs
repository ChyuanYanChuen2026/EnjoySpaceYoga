using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>()
            .Property(course => course.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Teacher>()
            .Property(teacher => teacher.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Location>()
            .Property(location => location.RowVersion)
            .IsRowVersion();

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
            .HasOne(session => session.Location)
            .WithMany()
            .HasForeignKey(session => session.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClassSession>()
            .Property(session => session.RowVersion)
            .IsRowVersion();
    }
}