using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                schema: "dbo",
                table: "Students",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Students_UserId_Transition",
                schema: "dbo",
                table: "Students",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users",
                schema: "dbo",
                table: "Students",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "UX_Students_UserId_Transition",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dbo",
                table: "Students");
        }
    }
}
