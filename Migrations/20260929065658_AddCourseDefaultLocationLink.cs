using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseDefaultLocationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DefaultLocationId",
                schema: "dbo",
                table: "Courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DefaultLocationId",
                schema: "dbo",
                table: "Courses",
                column: "DefaultLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_DefaultLocation",
                schema: "dbo",
                table: "Courses",
                column: "DefaultLocationId",
                principalSchema: "dbo",
                principalTable: "StudioLocations",
                principalColumn: "StudioLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_DefaultLocation",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_DefaultLocationId",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DefaultLocationId",
                schema: "dbo",
                table: "Courses");
        }
    }
}
