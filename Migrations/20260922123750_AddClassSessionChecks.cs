using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSessionChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_ClassSessions_TimeRange",
                schema: "dbo",
                table: "ClassSessions",
                sql: "[EndsAt] > [StartsAt]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClassSessions_Capacity",
                schema: "dbo",
                table: "ClassSessions",
                sql: "[Capacity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClassSessions_CapacityOverride",
                schema: "dbo",
                table: "ClassSessions",
                sql: "[CapacityOverride] IS NULL OR [CapacityOverride] >= [Capacity]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ClassSessions_CapacityOverride",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClassSessions_Capacity",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClassSessions_TimeRange",
                schema: "dbo",
                table: "ClassSessions");
        }
    }
}
