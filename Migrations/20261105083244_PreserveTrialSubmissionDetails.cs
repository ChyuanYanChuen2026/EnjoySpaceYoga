using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class PreserveTrialSubmissionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubmittedAccount",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedEmail",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedLineId",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedName",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedPhone",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedAccount",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "SubmittedEmail",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "SubmittedLineId",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "SubmittedName",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "SubmittedPhone",
                schema: "dbo",
                table: "PortalTrialRequests");
        }
    }
}
