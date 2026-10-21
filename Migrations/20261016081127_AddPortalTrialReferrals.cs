using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalTrialReferrals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferrerName",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReferrer",
                schema: "dbo",
                table: "LessonPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferrerName",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "RequiresReferrer",
                schema: "dbo",
                table: "LessonPlans");
        }
    }
}
