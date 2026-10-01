using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                schema: "dbo",
                table: "Teachers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_Teachers_UserId_Transition",
                schema: "dbo",
                table: "Teachers",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Users",
                schema: "dbo",
                table: "Teachers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Users",
                schema: "dbo",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "UX_Teachers_UserId_Transition",
                schema: "dbo",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dbo",
                table: "Teachers");
        }
    }
}
