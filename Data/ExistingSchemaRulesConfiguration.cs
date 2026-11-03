using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

// These rules already exist in SQL Server and historical migrations. Keep the
// model (including EnsureCreated test databases) consistent with that schema.
internal static class ExistingSchemaRulesConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        model.Entity<ClassSession>().ToTable("ClassSessions", "dbo", table =>
        {
            table.HasCheckConstraint("CK_ClassSessions_TimeRange", "[EndsAt] > [StartsAt]");
            table.HasCheckConstraint("CK_ClassSessions_Capacity", "[Capacity] > 0");
            table.HasCheckConstraint("CK_ClassSessions_CapacityOverride", "[CapacityOverride] IS NULL OR [CapacityOverride] >= [Capacity]");
        });
        model.Entity<Course>().ToTable("Courses", "dbo", table =>
        {
            table.HasCheckConstraint("CK_Courses_DefaultCapacity", "[DefaultCapacity] > 0");
            table.HasCheckConstraint("CK_Courses_TermPrice", "[TermPrice] >= 0");
        });
        model.Entity<StudioLocation>().ToTable("StudioLocations", "dbo", table =>
        {
            table.HasCheckConstraint("CK_StudioLocations_Radius", "[CheckInRadiusMeters] > 0");
            table.HasCheckConstraint("CK_StudioLocations_Latitude", "[Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)");
            table.HasCheckConstraint("CK_StudioLocations_Longitude", "[Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)");
        });
        model.Entity<Term>().ToTable("Terms", "dbo", table =>
            table.HasCheckConstraint("CK_Terms_DateRange", "[StartDate] <= [EndDate]"));

        model.Entity<ClassSession>().Property(x => x.IsExtraSession).HasDefaultValue(false);
        model.Entity<LessonPlan>().Property(x => x.RequiresReferrer).HasDefaultValue(false);
        model.Entity<EnrollmentApplication>().Property(x => x.ApplicationType).HasDefaultValue("FullTerm");
        model.Entity<EnrollmentApplication>().Property(x => x.QuotedAmount).HasDefaultValue(0m);
        // Historical default only; CK_EnrollmentApplications_QuotedSessionCount
        // still requires the caller to supply a positive count for a valid quote.
        model.Entity<EnrollmentApplication>().Property(x => x.QuotedSessionCount).HasDefaultValue((short)0);
    }
}
