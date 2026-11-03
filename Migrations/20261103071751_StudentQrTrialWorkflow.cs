using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class StudentQrTrialWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassSessionParticipants_TrialRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.AddColumn<short>(
                name: "QuotedQuota",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuotedValidityUnit",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuotedValidityValue",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReferrerAtRequest",
                schema: "dbo",
                table: "PortalTrialRequests",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TrialBookingId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CheckInEvents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ClassSessionId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AttendanceStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IsError = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ResolvedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckInEvents_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CheckInEvents_Users_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_CheckInEvents_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SessionQrCodes",
                schema: "dbo",
                columns: table => new
                {
                    ClassSessionId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProtectedToken = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    OpensAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionQrCodes", x => x.ClassSessionId);
                    table.ForeignKey(
                        name: "FK_SessionQrCodes_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TrialBookings",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrialRequestId = table.Column<long>(type: "bigint", nullable: false),
                    ClassSessionId = table.Column<int>(type: "int", nullable: false),
                    BatchKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialBookings", x => x.Id);
                    table.CheckConstraint("CK_TrialBookings_Status", "[Status] IN (N'Pending',N'Approved',N'Rejected',N'Cancelled')");
                    table.ForeignKey(
                        name: "FK_TrialBookings_ClassSessions_ClassSessionId",
                        column: x => x.ClassSessionId,
                        principalSchema: "dbo",
                        principalTable: "ClassSessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrialBookings_PortalTrialRequests_TrialRequestId",
                        column: x => x.TrialRequestId,
                        principalSchema: "dbo",
                        principalTable: "PortalTrialRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrialBookings_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_TrialBookingId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "TrialBookingId",
                unique: true,
                filter: "[TrialBookingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_TrialRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "TrialRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInEvents_ClassSessionId_CreatedAt",
                schema: "dbo",
                table: "CheckInEvents",
                columns: new[] { "ClassSessionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInEvents_ResolvedByUserId",
                schema: "dbo",
                table: "CheckInEvents",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInEvents_UserId",
                schema: "dbo",
                table: "CheckInEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionQrCodes_TokenHash",
                schema: "dbo",
                table: "SessionQrCodes",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrialBookings_ClassSessionId",
                schema: "dbo",
                table: "TrialBookings",
                column: "ClassSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialBookings_ReviewedByUserId",
                schema: "dbo",
                table: "TrialBookings",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialBookings_TrialRequestId_BatchKey_ClassSessionId",
                schema: "dbo",
                table: "TrialBookings",
                columns: new[] { "TrialRequestId", "BatchKey", "ClassSessionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrialBookings_TrialRequestId_ClassSessionId",
                schema: "dbo",
                table: "TrialBookings",
                columns: new[] { "TrialRequestId", "ClassSessionId" },
                unique: true,
                filter: "[Status] IN (N'Pending',N'Approved')");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessionParticipants_TrialBookings_TrialBookingId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "TrialBookingId",
                principalSchema: "dbo",
                principalTable: "TrialBookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessionParticipants_TrialBookings_TrialBookingId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.DropTable(
                name: "CheckInEvents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SessionQrCodes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TrialBookings",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessionParticipants_TrialBookingId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessionParticipants_TrialRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.DropColumn(
                name: "QuotedQuota",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "QuotedValidityUnit",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "QuotedValidityValue",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "RequiresReferrerAtRequest",
                schema: "dbo",
                table: "PortalTrialRequests");

            migrationBuilder.DropColumn(
                name: "TrialBookingId",
                schema: "dbo",
                table: "ClassSessionParticipants");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionParticipants_TrialRequestId",
                schema: "dbo",
                table: "ClassSessionParticipants",
                column: "TrialRequestId",
                unique: true,
                filter: "[TrialRequestId] IS NOT NULL");
        }
    }
}
