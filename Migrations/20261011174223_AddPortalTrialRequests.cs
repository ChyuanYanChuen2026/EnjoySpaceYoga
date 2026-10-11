using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalTrialRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleCode = N'TrialGuest') INSERT INTO dbo.Roles (RoleCode, RoleName, IsActive) VALUES (N'TrialGuest', N'體驗生', 1);");
            migrationBuilder.CreateTable(
                name: "PortalTrialRequests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TrialGuestId = table.Column<long>(type: "bigint", nullable: false),
                    ClassSessionId = table.Column<int>(type: "int", nullable: false),
                    LessonPlanId = table.Column<long>(type: "bigint", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotedPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalTrialRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortalTrialRequests_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PortalTrialRequests_LessonPlans_LessonPlanId",
                        column: x => x.LessonPlanId,
                        principalSchema: "dbo",
                        principalTable: "LessonPlans",
                        principalColumn: "LessonPlanId");
                    table.ForeignKey(
                        name: "FK_PortalTrialRequests_TrialGuests_TrialGuestId",
                        column: x => x.TrialGuestId,
                        principalSchema: "dbo",
                        principalTable: "TrialGuests",
                        principalColumn: "TrialGuestId");
                    table.ForeignKey(
                        name: "FK_PortalTrialRequests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortalTrialRequests_ClassSessionId",
                schema: "dbo",
                table: "PortalTrialRequests",
                column: "ClassSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PortalTrialRequests_LessonPlanId",
                schema: "dbo",
                table: "PortalTrialRequests",
                column: "LessonPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PortalTrialRequests_SubmissionKey",
                schema: "dbo",
                table: "PortalTrialRequests",
                column: "SubmissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortalTrialRequests_TrialGuestId",
                schema: "dbo",
                table: "PortalTrialRequests",
                column: "TrialGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PortalTrialRequests_UserId",
                schema: "dbo",
                table: "PortalTrialRequests",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortalTrialRequests",
                schema: "dbo");
        }
    }
}
