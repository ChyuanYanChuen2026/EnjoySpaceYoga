using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "dbo",
                table: "Teachers",
                type: "datetime2(0)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()")
                .Annotation("Relational:DefaultConstraintName", "DF_Teachers_CreatedAtUtc");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Teachers",
                type: "bit",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:DefaultConstraintName", "DF_Teachers_IsActive");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "dbo",
                table: "Teachers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherNo",
                schema: "dbo",
                table: "Teachers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Teachers_TeacherNo",
                schema: "dbo",
                table: "Teachers",
                column: "TeacherNo",
                unique: true,
                filter: "[TeacherNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Teachers_TeacherNo",
                schema: "dbo",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "dbo",
                table: "Teachers")
                .Annotation("Relational:DefaultConstraintName", "DF_Teachers_CreatedAtUtc");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Teachers")
                .Annotation("Relational:DefaultConstraintName", "DF_Teachers_IsActive");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "dbo",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "TeacherNo",
                schema: "dbo",
                table: "Teachers");
        }
    }
}
