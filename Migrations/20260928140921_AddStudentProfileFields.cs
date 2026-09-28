using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "dbo",
                table: "Students",
                type: "datetime2(0)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()")
                .Annotation("Relational:DefaultConstraintName", "DF_Students_CreatedAtUtc");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:DefaultConstraintName", "DF_Students_IsActive");

            migrationBuilder.AddColumn<DateOnly>(
                name: "JoinedDate",
                schema: "dbo",
                table: "Students",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentNo",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Students_StudentNo",
                schema: "dbo",
                table: "Students",
                column: "StudentNo",
                unique: true,
                filter: "[StudentNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Students_StudentNo",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "dbo",
                table: "Students")
                .Annotation("Relational:DefaultConstraintName", "DF_Students_CreatedAtUtc");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Students")
                .Annotation("Relational:DefaultConstraintName", "DF_Students_IsActive");

            migrationBuilder.DropColumn(
                name: "JoinedDate",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentNo",
                schema: "dbo",
                table: "Students");
        }
    }
}
