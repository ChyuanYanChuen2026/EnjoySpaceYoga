using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTrialPlansAndPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "dbo",
                table: "TrialGuests",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "LessonPlans",
                schema: "dbo",
                columns: table => new
                {
                    LessonPlanId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AudienceType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SessionQuota = table.Column<short>(type: "smallint", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValidityUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValidityValue = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlans", x => x.LessonPlanId);
                    table.CheckConstraint("CK_LessonPlans_Audience", "([PlanType] = N'Trial' AND [AudienceType] = N'TrialGuest') OR ([PlanType] = N'Flexible' AND [AudienceType] = N'Student')");
                    table.CheckConstraint("CK_LessonPlans_Price", "[Price] >= 0");
                    table.CheckConstraint("CK_LessonPlans_Quota", "[SessionQuota] > 0");
                    table.CheckConstraint("CK_LessonPlans_Validity", "([ValidityUnit] = N'Unlimited' AND [ValidityValue] IS NULL) OR ([ValidityUnit] = N'Days' AND [ValidityValue] IS NOT NULL AND [ValidityValue] > 0)");
                });

            migrationBuilder.CreateTable(
                name: "LessonPasses",
                schema: "dbo",
                columns: table => new
                {
                    LessonPassId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseKey = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonPlanId = table.Column<long>(type: "bigint", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: true),
                    TrialGuestId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentId = table.Column<long>(type: "bigint", nullable: false),
                    PlanNameAtPurchase = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlanTypeAtPurchase = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SessionQuotaAtPurchase = table.Column<short>(type: "smallint", nullable: false),
                    AmountAtPurchase = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ValidFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntilDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurchasedAtUtc = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPasses", x => x.LessonPassId);
                    table.CheckConstraint("CK_LessonPasses_Amount", "[AmountAtPurchase] >= 0");
                    table.CheckConstraint("CK_LessonPasses_DateRange", "[ValidUntilDate] IS NULL OR [ValidUntilDate] >= [ValidFromDate]");
                    table.CheckConstraint("CK_LessonPasses_Owner", "([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL) OR ([StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)");
                    table.CheckConstraint("CK_LessonPasses_Quota", "[SessionQuotaAtPurchase] > 0");
                    table.CheckConstraint("CK_LessonPasses_Status", "[Status] IN (N'Pending', N'Active', N'Cancelled')");
                    table.ForeignKey(
                        name: "FK_LessonPasses_LessonPlans_LessonPlanId",
                        column: x => x.LessonPlanId,
                        principalSchema: "dbo",
                        principalTable: "LessonPlans",
                        principalColumn: "LessonPlanId");
                    table.ForeignKey(
                        name: "FK_LessonPasses_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "dbo",
                        principalTable: "Payments",
                        principalColumn: "PaymentId");
                    table.ForeignKey(
                        name: "FK_LessonPasses_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "dbo",
                        principalTable: "Students",
                        principalColumn: "StudentId");
                    table.ForeignKey(
                        name: "FK_LessonPasses_TrialGuests_TrialGuestId",
                        column: x => x.TrialGuestId,
                        principalSchema: "dbo",
                        principalTable: "TrialGuests",
                        principalColumn: "TrialGuestId");
                    table.ForeignKey(
                        name: "FK_LessonPasses_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonPasses_CreatedByUserId",
                schema: "dbo",
                table: "LessonPasses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPasses_LessonPlanId",
                schema: "dbo",
                table: "LessonPasses",
                column: "LessonPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPasses_PaymentId",
                schema: "dbo",
                table: "LessonPasses",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPasses_PurchaseKey",
                schema: "dbo",
                table: "LessonPasses",
                column: "PurchaseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPasses_StudentId",
                schema: "dbo",
                table: "LessonPasses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPasses_TrialGuestId",
                schema: "dbo",
                table: "LessonPasses",
                column: "TrialGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlans_PlanCode",
                schema: "dbo",
                table: "LessonPlans",
                column: "PlanCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonPasses",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LessonPlans",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "dbo",
                table: "TrialGuests");
        }
    }
}
