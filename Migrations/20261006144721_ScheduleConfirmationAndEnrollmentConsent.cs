using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleConfirmationAndEnrollmentConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleConfirmedAtUtc",
                schema: "dbo",
                table: "Terms",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ScheduleConfirmedByUserId",
                schema: "dbo",
                table: "Terms",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedPolicy",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PolicyConfirmedAtUtc",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "datetime2(0)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PolicyConfirmedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleConfirmedAtUtc",
                schema: "dbo",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "ScheduleConfirmedByUserId",
                schema: "dbo",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "ConfirmedPolicy",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "PolicyConfirmedAtUtc",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "PolicyConfirmedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications");
        }
    }
}
