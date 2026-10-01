using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTermLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TermId",
                schema: "dbo",
                table: "Courses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TermId",
                schema: "dbo",
                table: "Courses",
                column: "TermId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Terms",
                schema: "dbo",
                table: "Courses",
                column: "TermId",
                principalSchema: "dbo",
                principalTable: "Terms",
                principalColumn: "TermId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Terms",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TermId",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TermId",
                schema: "dbo",
                table: "Courses");
        }
    }
}
