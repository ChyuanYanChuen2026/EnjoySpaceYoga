using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveAndMakeupWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.AddColumn<long>(
                name: "MakeupRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                schema: "dbo",
                columns: table => new
                {
                    LeaveRequestId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionParticipantId = table.Column<long>(type: "bigint", nullable: false),
                    RequestSource = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    CancelledByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.LeaveRequestId);
                    table.CheckConstraint("CK_LeaveRequests_Status", "[Status] IN (N'Active', N'Cancelled')");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_ClassSessionParticipants_SessionParticipantId",
                        column: x => x.SessionParticipantId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessionParticipants",
                        principalColumn: "SessionParticipantId");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "MakeupEntitlements",
                schema: "dbo",
                columns: table => new
                {
                    MakeupEntitlementId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    SourceTermId = table.Column<long>(type: "bigint", nullable: false),
                    SourceLeaveRequestId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ConsumedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupEntitlements", x => x.MakeupEntitlementId);
                    table.CheckConstraint("CK_MakeupEntitlements_Status", "[Status] IN (N'Available', N'Reserved', N'Consumed', N'Revoked')");
                    table.ForeignKey(
                        name: "FK_MakeupEntitlements_LeaveRequests_SourceLeaveRequestId",
                        column: x => x.SourceLeaveRequestId,
                        principalSchema: "dbo",
                        principalTable: "LeaveRequests",
                        principalColumn: "LeaveRequestId");
                    table.ForeignKey(
                        name: "FK_MakeupEntitlements_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_MakeupEntitlements_Terms_SourceTermId",
                        column: x => x.SourceTermId,
                        principalSchema: "dbo",
                        principalTable: "Terms",
                        principalColumn: "TermId");
                });

            migrationBuilder.CreateTable(
                name: "MakeupRequests",
                schema: "dbo",
                columns: table => new
                {
                    MakeupRequestId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MakeupEntitlementId = table.Column<long>(type: "bigint", nullable: false),
                    TargetClassSessionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupRequests", x => x.MakeupRequestId);
                    table.CheckConstraint("CK_MakeupRequests_Status", "[Status] IN (N'Pending', N'Approved', N'Rejected', N'Cancelled')");
                    table.ForeignKey(
                        name: "FK_MakeupRequests_ClassSessions_TargetClassSessionId",
                        column: x => x.TargetClassSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MakeupRequests_MakeupEntitlements_MakeupEntitlementId",
                        column: x => x.MakeupEntitlementId,
                        principalSchema: "dbo",
                        principalTable: "MakeupEntitlements",
                        principalColumn: "MakeupEntitlementId");
                    table.ForeignKey(
                        name: "FK_MakeupRequests_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MakeupRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_MakeupRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                columns: new[] { "ClassSessionId", "CourseEnrollmentId" },
                unique: true,
                filter: "[ParticipationType] = N'Regular'");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_MakeupRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "MakeupRequestId",
                unique: true,
                filter: "[MakeupRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_CancelledByUserId",
                schema: "dbo",
                table: "LeaveRequests",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_CreatedByUserId",
                schema: "dbo",
                table: "LeaveRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_SessionParticipantId",
                schema: "dbo",
                table: "LeaveRequests",
                column: "SessionParticipantId",
                unique: true,
                filter: "[Status] = N'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupEntitlements_SourceLeaveRequestId",
                schema: "dbo",
                table: "MakeupEntitlements",
                column: "SourceLeaveRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MakeupEntitlements_SourceTermId",
                schema: "dbo",
                table: "MakeupEntitlements",
                column: "SourceTermId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupEntitlements_StudentId",
                schema: "dbo",
                table: "MakeupEntitlements",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupRequests_CancelledByUserId",
                schema: "dbo",
                table: "MakeupRequests",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupRequests_MakeupEntitlementId_Status",
                schema: "dbo",
                table: "MakeupRequests",
                columns: new[] { "MakeupEntitlementId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MakeupRequests_RequestedByUserId",
                schema: "dbo",
                table: "MakeupRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupRequests_ReviewedByUserId",
                schema: "dbo",
                table: "MakeupRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MakeupRequests_TargetClassSessionId",
                schema: "dbo",
                table: "MakeupRequests",
                column: "TargetClassSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessionParticipants_MakeupRequests_MakeupRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "MakeupRequestId",
                principalSchema: "dbo",
                principalTable: "MakeupRequests",
                principalColumn: "MakeupRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessionParticipants_MakeupRequests_MakeupRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.DropTable(
                name: "MakeupRequests",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MakeupEntitlements",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LeaveRequests",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessionParticipants_MakeupRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.DropColumn(
                name: "MakeupRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                columns: new[] { "ClassSessionId", "CourseEnrollmentId" },
                unique: true);
        }
    }
}
