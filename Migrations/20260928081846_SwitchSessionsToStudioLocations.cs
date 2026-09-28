using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class SwitchSessionsToStudioLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM dbo.ClassSessions
                    WHERE StudioLocationId IS NULL
                )
                BEGIN
                    THROW 50003, N'仍有場次未設定正式地點，停止切換。', 1;
                END;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_Locations_LocationId",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_LocationId",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.AlterColumn<long>(
                name: "StudioLocationId",
                schema: "dbo",
                table: "ClassSessions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM dbo.Locations WHERE Id = 1
                )
                BEGIN
                    THROW 50004, N'舊地點 1 不存在，無法回復。', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ClassSessions
                    WHERE StudioLocationId IS NULL
                       OR StudioLocationId <> 1
                )
                BEGIN
                    THROW 50005, N'存在其他正式地點，需先確認回復對應。', 1;
                END;
                """);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                schema: "dbo",
                table: "ClassSessions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
        UPDATE dbo.ClassSessions
        SET LocationId = 1
        WHERE StudioLocationId = 1;
        """);

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                schema: "dbo",
                table: "ClassSessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_LocationId",
                schema: "dbo",
                table: "ClassSessions",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_Locations_LocationId",
                schema: "dbo",
                table: "ClassSessions",
                column: "LocationId",
                principalSchema: "dbo",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AlterColumn<long>(
                name: "StudioLocationId",
                schema: "dbo",
                table: "ClassSessions",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
