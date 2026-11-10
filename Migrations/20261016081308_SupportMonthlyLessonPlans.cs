using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class SupportMonthlyLessonPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LessonPlans_Validity",
                schema: "dbo",
                table: "LessonPlans");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LessonPlans_Validity",
                schema: "dbo",
                table: "LessonPlans",
                sql: "([ValidityUnit] = N'Unlimited' AND [ValidityValue] IS NULL) OR ([ValidityUnit] IN (N'Days', N'Months') AND [ValidityValue] IS NOT NULL AND [ValidityValue] > 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LessonPlans_Validity",
                schema: "dbo",
                table: "LessonPlans");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LessonPlans_Validity",
                schema: "dbo",
                table: "LessonPlans",
                sql: "([ValidityUnit] = N'Unlimited' AND [ValidityValue] IS NULL) OR ([ValidityUnit] = N'Days' AND [ValidityValue] IS NOT NULL AND [ValidityValue] > 0)");
        }
    }
}
