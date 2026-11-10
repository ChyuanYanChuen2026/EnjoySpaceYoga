using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionParticipantsAndAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassSessionParticipants",
                schema: "dbo",
                columns: table => new
                {
                    SessionParticipantId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassSessionId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    CourseEnrollmentId = table.Column<long>(type: "bigint", nullable: false),
                    ParticipationType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AddedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSessionParticipants", x => x.SessionParticipantId);
                    table.ForeignKey(
                        name: "FK_ClassSessionParticipants_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassSessionParticipants_CourseEnrollments_CourseEnrollmentId",
                        column: x => x.CourseEnrollmentId,
                        principalSchema: "dbo",
                        principalTable: "CourseEnrollments",
                        principalColumn: "CourseEnrollmentId");
                    table.ForeignKey(
                        name: "FK_ClassSessionParticipants_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_ClassSessionParticipants_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                schema: "dbo",
                columns: table => new
                {
                    AttendanceRecordId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionParticipantId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CheckInMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CheckedInByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ManualReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.AttendanceRecordId);
                    table.CheckConstraint("CK_AttendanceRecords_Status", "[Status] IN (N'Scheduled', N'Present', N'Late', N'Absent')");
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_ClassSessionParticipants_SessionParticipantId",
                        column: x => x.SessionParticipantId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessionParticipants",
                        principalColumn: "SessionParticipantId");
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Users_CheckedInByUserId",
                        column: x => x.CheckedInByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_CheckedInByUserId",
                schema: "dbo",
                table: "AttendanceRecords",
                column: "CheckedInByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_SessionParticipantId",
                schema: "dbo",
                table: "AttendanceRecords",
                column: "SessionParticipantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_Status",
                schema: "dbo",
                table: "AttendanceRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_AddedByUserId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                columns: new[] { "ClassSessionId", "CourseEnrollmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_CourseEnrollmentId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "CourseEnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_StudentId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "UX_ClassSessionParticipants_ActiveStudent",
                schema: "dbo",
                table: "ClassSessionParticipants",
                columns: new[] { "ClassSessionId", "StudentId" },
                unique: true,
                filter: "[Status] = N'Active'");

            // Existing approved full-term enrollments need the same roster as new approvals.
            // Leave historical attendance unknown; never infer presence from enrollment/payment.
            migrationBuilder.Sql("""
                INSERT INTO dbo.ClassSessionParticipants
                    (ClassSessionId, StudentId, CourseEnrollmentId, ParticipationType, Status, AddedByUserId, AddedAtUtc)
                SELECT s.Id, e.StudentId, e.CourseEnrollmentId, N'Regular', N'Active', e.CreatedByUserId, SYSUTCDATETIME()
                FROM dbo.CourseEnrollments e
                JOIN dbo.EnrollmentApplications a ON a.EnrollmentApplicationId = e.EnrollmentApplicationId
                JOIN dbo.ClassSessions s ON s.CourseId = e.CourseId
                WHERE e.Status = N'Active' AND a.Status = N'Approved';

                INSERT INTO dbo.AttendanceRecords (SessionParticipantId, Status)
                SELECT SessionParticipantId, N'Scheduled' FROM dbo.ClassSessionParticipants;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ClassSessionParticipants",
                schema: "dbo");
        }
    }
}
