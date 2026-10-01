using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentPaymentWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET ANSI_NULLS ON;
                SET ANSI_PADDING ON;
                SET ANSI_WARNINGS ON;
                SET ARITHABORT ON;
                SET CONCAT_NULL_YIELDS_NULL ON;
                SET QUOTED_IDENTIFIER ON;
                SET NUMERIC_ROUNDABORT OFF;
                IF EXISTS (SELECT 1 FROM dbo.EnrollmentApplications WHERE Status = 2)
                    THROW 50101, N'已有核准的舊申請，需先核對收款與建立者，不能推定已付款。', 1;
                IF EXISTS (
                    SELECT 1 FROM dbo.EnrollmentApplications a
                    OUTER APPLY (SELECT COUNT(*) AS Total FROM dbo.ClassSessions s WHERE s.CourseId = a.CourseId) q
                    WHERE q.Total < 1 OR q.Total > 32767)
                    THROW 50102, N'舊報名所屬課程沒有有效堂數，停止搬移。', 1;
                ALTER TABLE dbo.EnrollmentApplications DROP CONSTRAINT CK_EnrollmentApplications_Status;
                ALTER TABLE dbo.EnrollmentApplications DROP CONSTRAINT PK_EnrollmentApplications;
                UPDATE dbo.EnrollmentApplications
                SET SubmittedAt = SWITCHOFFSET(SubmittedAt, '+00:00'),
                    ReviewedAt = SWITCHOFFSET(ReviewedAt, '+00:00');
                """);
            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                schema: "dbo",
                table: "EnrollmentApplications",
                newName: "AppliedAtUtc");

            migrationBuilder.RenameColumn(
                name: "ReviewedAt",
                schema: "dbo",
                table: "EnrollmentApplications",
                newName: "ReviewedAtUtc");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "dbo",
                table: "EnrollmentApplications",
                newName: "EnrollmentApplicationId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedAtUtc",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "datetime2(0)",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReviewedAtUtc",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "datetime2(0)",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "EnrollmentApplicationId",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationType",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "FullTerm");

            migrationBuilder.AddColumn<int>(
                name: "FirstSessionId",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentId",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuotedAmount",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<short>(
                name: "QuotedSessionCount",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<long>(
                name: "ReviewedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE dbo.EnrollmentApplications ADD CONSTRAINT PK_EnrollmentApplications PRIMARY KEY (EnrollmentApplicationId);
                UPDATE dbo.EnrollmentApplications
                SET Status = CASE Status WHEN N'1' THEN N'Pending' WHEN N'2' THEN N'Approved'
                    WHEN N'3' THEN N'Rejected' WHEN N'4' THEN N'Cancelled' ELSE Status END;
                UPDATE a SET QuotedAmount = c.TermPrice, QuotedSessionCount = q.Total
                FROM dbo.EnrollmentApplications a
                INNER JOIN dbo.Courses c ON c.CourseId = a.CourseId
                CROSS APPLY (SELECT COUNT(*) AS Total FROM dbo.ClassSessions s WHERE s.CourseId = a.CourseId) q;
                """);

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                schema: "dbo",
                columns: table => new
                {
                    CourseEnrollmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    EnrollmentApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EnrolledAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => x.CourseEnrollmentId);
                    table.UniqueConstraint("UQ_CourseEnrollments_Application", x => x.EnrollmentApplicationId);
                    table.UniqueConstraint("UQ_CourseEnrollments_Course_Student", x => new { x.CourseId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "dbo",
                        principalTable: "Courses",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_EnrollmentApplications_EnrollmentApplicationId",
                        column: x => x.EnrollmentApplicationId,
                        principalSchema: "dbo",
                        principalTable: "EnrollmentApplications",
                        principalColumn: "EnrollmentApplicationId");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TrialGuests",
                schema: "dbo",
                columns: table => new
                {
                    TrialGuestId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LineId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferrerStudentId = table.Column<long>(type: "bigint", nullable: true),
                    ConvertedStudentId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialGuests", x => x.TrialGuestId);
                    table.ForeignKey(
                        name: "FK_TrialGuests_Students_ConvertedStudentId",
                        column: x => x.ConvertedStudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_TrialGuests_Students_ReferrerStudentId",
                        column: x => x.ReferrerStudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_TrialGuests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "dbo",
                columns: table => new
                {
                    PaymentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: true),
                    TrialGuestId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentPurpose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ConfirmedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.CheckConstraint("CK_Payments_Amount", "[Amount] >= 0");
                    table.CheckConstraint("CK_Payments_Payer", "([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL) OR ([StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Payments_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_Payments_TrialGuests_TrialGuestId",
                        column: x => x.TrialGuestId,
                        principalSchema: "dbo",
                        principalTable: "TrialGuests",
                        principalColumn: "TrialGuestId");
                    table.ForeignKey(
                        name: "FK_Payments_Users_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.Sql("""
                DECLARE @mapping TABLE (ApplicationId bigint, PaymentId bigint);
                MERGE dbo.Payments AS target
                USING (SELECT EnrollmentApplicationId, StudentId, QuotedAmount FROM dbo.EnrollmentApplications WHERE Status = N'Pending') AS source
                ON 1 = 0
                WHEN NOT MATCHED THEN INSERT (StudentId, PaymentPurpose, Amount, Status, CreatedAtUtc, Note)
                    VALUES (source.StudentId, N'CourseEnrollment', source.QuotedAmount, N'Pending', SYSUTCDATETIME(),
                        N'舊申請搬移：依目前課程價格及堂數補報價，尚未確認收款。')
                OUTPUT source.EnrollmentApplicationId, inserted.PaymentId INTO @mapping;
                UPDATE a SET PaymentId = m.PaymentId
                FROM dbo.EnrollmentApplications a INNER JOIN @mapping m ON a.EnrollmentApplicationId = m.ApplicationId;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentApplications_FirstSessionId",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "FirstSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentApplications_PaymentId",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentApplications_ReviewedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentApplications_Status",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "Status");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EnrollmentApplications_QuotedAmount",
                schema: "dbo",
                table: "EnrollmentApplications",
                sql: "[QuotedAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EnrollmentApplications_QuotedSessionCount",
                schema: "dbo",
                table: "EnrollmentApplications",
                sql: "[QuotedSessionCount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EnrollmentApplications_Status",
                schema: "dbo",
                table: "EnrollmentApplications",
                sql: "[Status] IN (N'Pending',N'Approved',N'Rejected',N'Cancelled')");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CreatedByUserId",
                schema: "dbo",
                table: "CourseEnrollments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentId",
                schema: "dbo",
                table: "CourseEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ConfirmedByUserId",
                schema: "dbo",
                table: "Payments",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                schema: "dbo",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                schema: "dbo",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TrialGuestId",
                schema: "dbo",
                table: "Payments",
                column: "TrialGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialGuests_ConvertedStudentId",
                schema: "dbo",
                table: "TrialGuests",
                column: "ConvertedStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialGuests_CreatedByUserId",
                schema: "dbo",
                table: "TrialGuests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialGuests_Email",
                schema: "dbo",
                table: "TrialGuests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_TrialGuests_Phone",
                schema: "dbo",
                table: "TrialGuests",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_TrialGuests_ReferrerStudentId",
                schema: "dbo",
                table: "TrialGuests",
                column: "ReferrerStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentApplications_ClassSessions_FirstSessionId",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "FirstSessionId",
                principalSchema: "dbo",
                principalTable: "ClassSessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentApplications_Payments_PaymentId",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "PaymentId",
                principalSchema: "dbo",
                principalTable: "Payments",
                principalColumn: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentApplications_Users_ReviewedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications",
                column: "ReviewedByUserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM dbo.Payments) OR EXISTS (SELECT 1 FROM dbo.CourseEnrollments) OR EXISTS (SELECT 1 FROM dbo.TrialGuests)
                    THROW 50103, N'已有付款、正式報名或體驗資料，禁止直接回復以免遺失紀錄。請使用資料庫備份。', 1;
                IF EXISTS (SELECT 1 FROM dbo.EnrollmentApplications WHERE EnrollmentApplicationId > 2147483647)
                    THROW 50104, N'申請編號超出 int 範圍，無法回復。', 1;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentApplications_ClassSessions_FirstSessionId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentApplications_Payments_PaymentId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentApplications_Users_ReviewedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropTable(
                name: "CourseEnrollments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TrialGuests",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_EnrollmentApplications_FirstSessionId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropIndex(
                name: "IX_EnrollmentApplications_PaymentId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropIndex(
                name: "IX_EnrollmentApplications_ReviewedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropIndex(
                name: "IX_EnrollmentApplications_Status",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EnrollmentApplications_QuotedAmount",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EnrollmentApplications_QuotedSessionCount",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EnrollmentApplications_Status",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "ApplicationType",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "FirstSessionId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "QuotedAmount",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "QuotedSessionCount",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "dbo",
                table: "EnrollmentApplications");

            migrationBuilder.Sql("""
                ALTER TABLE dbo.EnrollmentApplications DROP CONSTRAINT PK_EnrollmentApplications;
                UPDATE dbo.EnrollmentApplications SET Status = CASE Status
                    WHEN N'Pending' THEN N'1' WHEN N'Approved' THEN N'2'
                    WHEN N'Rejected' THEN N'3' WHEN N'Cancelled' THEN N'4' END;
                """);
            migrationBuilder.RenameColumn(
                name: "ReviewedAtUtc",
                schema: "dbo",
                table: "EnrollmentApplications",
                newName: "ReviewedAt");

            migrationBuilder.RenameColumn(
                name: "AppliedAtUtc",
                schema: "dbo",
                table: "EnrollmentApplications",
                newName: "SubmittedAt");

            migrationBuilder.RenameColumn(
                name: "EnrollmentApplicationId",
                schema: "dbo",
                table: "EnrollmentApplications",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SubmittedAt",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "dbo",
                table: "EnrollmentApplications",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");
            migrationBuilder.Sql("""
                ALTER TABLE dbo.EnrollmentApplications ADD CONSTRAINT PK_EnrollmentApplications PRIMARY KEY (Id);
                ALTER TABLE dbo.EnrollmentApplications ADD CONSTRAINT CK_EnrollmentApplications_Status CHECK (Status IN (1,2,3,4));
                """);
        }
    }
}
