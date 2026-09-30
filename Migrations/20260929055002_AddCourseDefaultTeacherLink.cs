using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseDefaultTeacherLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DefaultTeacherId",
                schema: "dbo",
                table: "Courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DefaultTeacherId",
                schema: "dbo",
                table: "Courses",
                column: "DefaultTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_DefaultTeacher",
                schema: "dbo",
                table: "Courses",
                column: "DefaultTeacherId",
                principalSchema: "dbo",
                principalTable: "Teachers",
                principalColumn: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_DefaultTeacher",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_DefaultTeacherId",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DefaultTeacherId",
                schema: "dbo",
                table: "Courses");
        }
    }
}
