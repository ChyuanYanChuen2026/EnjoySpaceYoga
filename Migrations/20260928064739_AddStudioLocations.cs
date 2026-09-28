using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddStudioLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudioLocations",
                schema: "dbo",
                columns: table => new
                {
                    StudioLocationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    CheckInRadiusMeters = table.Column<int>(type: "int", nullable: false, defaultValue: 500)
                        .Annotation("Relational:DefaultConstraintName", "DF_StudioLocations_Radius"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                        .Annotation("Relational:DefaultConstraintName", "DF_StudioLocations_IsActive"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "DF_StudioLocations_CreatedAtUtc")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudioLocations", x => x.StudioLocationId);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_StudioLocations_Radius",
                schema: "dbo",
                table: "StudioLocations",
                sql: "[CheckInRadiusMeters] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StudioLocations_Latitude",
                schema: "dbo",
                table: "StudioLocations",
                sql: "[Latitude] IS NULL OR ([Latitude] >= -90 AND [Latitude] <= 90)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StudioLocations_Longitude",
                schema: "dbo",
                table: "StudioLocations",
                sql: "[Longitude] IS NULL OR ([Longitude] >= -180 AND [Longitude] <= 180)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudioLocations",
                schema: "dbo");
        }
    }
}
