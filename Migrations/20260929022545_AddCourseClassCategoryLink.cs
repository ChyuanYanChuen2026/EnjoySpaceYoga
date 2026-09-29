using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseClassCategoryLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassCategoryId",
                schema: "dbo",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ClassCategoryId",
                schema: "dbo",
                table: "Courses",
                column: "ClassCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_ClassCategories",
                schema: "dbo",
                table: "Courses",
                column: "ClassCategoryId",
                principalSchema: "dbo",
                principalTable: "ClassCategories",
                principalColumn: "ClassCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_ClassCategories",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ClassCategoryId",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ClassCategoryId",
                schema: "dbo",
                table: "Courses");
        }
    }
}
