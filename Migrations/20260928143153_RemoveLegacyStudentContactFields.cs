using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyStudentContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "dbo",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "dbo",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "dbo",
                table: "Students",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE s
                SET
                    s.Name = u.DisplayName,
                    s.Phone = u.Phone,
                    s.Email = u.Email
                FROM dbo.Students AS s
                INNER JOIN dbo.Users AS u
                    ON s.UserId = u.UserId;
                """);
        }
    }
}
