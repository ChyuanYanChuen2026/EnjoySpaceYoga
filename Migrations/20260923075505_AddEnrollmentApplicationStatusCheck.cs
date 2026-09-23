using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentApplicationStatusCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_EnrollmentApplications_Status",
                schema: "dbo",
                table: "EnrollmentApplications",
                sql: "[Status] IN (1, 2, 3, 4)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_EnrollmentApplications_Status",
                schema: "dbo",
                table: "EnrollmentApplications");
        }
    }
}
