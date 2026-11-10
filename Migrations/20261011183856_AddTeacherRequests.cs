using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExtraSession",
                schema: "dbo",
                table: "ClassSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TeacherRequests",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TeacherId = table.Column<long>(type: "bigint", nullable: false),
                    CourseId = table.Column<long>(type: "bigint", nullable: false),
                    TargetSessionId = table.Column<int>(type: "int", nullable: true),
                    CreatedSessionId = table.Column<int>(type: "int", nullable: true),
                    StudioLocationId = table.Column<long>(type: "bigint", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequestedCapacity = table.Column<int>(type: "int", nullable: false),
                    OriginalCapacity = table.Column<int>(type: "int", nullable: true),
                    TargetVersion = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherRequests", x => x.Id);
                    table.CheckConstraint("CK_TeacherRequests_Capacity", "[RequestedCapacity] BETWEEN 1 AND 32767");
                    table.CheckConstraint("CK_TeacherRequests_Kind", "([Kind] = N'ExtraSession' AND [TargetSessionId] IS NULL) OR ([Kind] = N'Capacity' AND [TargetSessionId] IS NOT NULL AND [OriginalCapacity] IS NOT NULL AND [TargetVersion] IS NOT NULL)");
                    table.CheckConstraint("CK_TeacherRequests_Status", "[Status] IN (N'Pending', N'Approved', N'Rejected')");
                    table.CheckConstraint("CK_TeacherRequests_Time", "[EndsAt] > [StartsAt]");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_ClassSessions_CreatedSessionId",
                        column: x => x.CreatedSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_ClassSessions_TargetSessionId",
                        column: x => x.TargetSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "dbo",
                        principalTable: "Courses",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_StudioLocations_StudioLocationId",
                        column: x => x.StudioLocationId,
                        principalSchema: "dbo",
                        principalTable: "StudioLocations",
                        principalColumn: "StudioLocationId");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalSchema: "dbo",
                        principalTable: "Teachers",
                        principalColumn: "TeacherId");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_TeacherRequests_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_CourseId",
                schema: "dbo",
                table: "TeacherRequests",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_CreatedSessionId",
                schema: "dbo",
                table: "TeacherRequests",
                column: "CreatedSessionId",
                unique: true,
                filter: "[CreatedSessionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_ReviewedByUserId",
                schema: "dbo",
                table: "TeacherRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_StudioLocationId",
                schema: "dbo",
                table: "TeacherRequests",
                column: "StudioLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_SubmissionKey",
                schema: "dbo",
                table: "TeacherRequests",
                column: "SubmissionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_SubmittedByUserId",
                schema: "dbo",
                table: "TeacherRequests",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_TargetSessionId",
                schema: "dbo",
                table: "TeacherRequests",
                column: "TargetSessionId",
                unique: true,
                filter: "[Kind] = N'Capacity' AND [Status] = N'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherRequests_TeacherId_Status",
                schema: "dbo",
                table: "TeacherRequests",
                columns: new[] { "TeacherId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherRequests",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "IsExtraSession",
                schema: "dbo",
                table: "ClassSessions");
        }
    }
}
