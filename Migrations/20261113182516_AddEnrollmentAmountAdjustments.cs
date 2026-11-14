using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentAmountAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnrollmentAmountAdjustments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrollmentApplicationId = table.Column<long>(type: "bigint", nullable: false),
                    PreviousAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AdjustedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    AdjustedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentAmountAdjustments", x => x.Id);
                    table.CheckConstraint("CK_EnrollmentAmountAdjustments_Amount", "[Amount] >= 0 AND [Amount] = FLOOR([Amount])");
                    table.ForeignKey(
                        name: "FK_EnrollmentAmountAdjustments_EnrollmentApplications_EnrollmentApplicationId",
                        column: x => x.EnrollmentApplicationId,
                        principalSchema: "dbo",
                        principalTable: "EnrollmentApplications",
                        principalColumn: "EnrollmentApplicationId");
                    table.ForeignKey(
                        name: "FK_EnrollmentAmountAdjustments_Users_AdjustedByUserId",
                        column: x => x.AdjustedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentAmountAdjustments_AdjustedByUserId",
                schema: "dbo",
                table: "EnrollmentAmountAdjustments",
                column: "AdjustedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentAmountAdjustments_EnrollmentApplicationId",
                schema: "dbo",
                table: "EnrollmentAmountAdjustments",
                column: "EnrollmentApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentAmountAdjustments",
                schema: "dbo");
        }
    }
}
