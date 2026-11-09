using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class LinkSessionsToStudioLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StudioLocationId",
                schema: "dbo",
                table: "ClassSessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_StudioLocationId",
                schema: "dbo",
                table: "ClassSessions",
                column: "StudioLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_Locations",
                schema: "dbo",
                table: "ClassSessions",
                column: "StudioLocationId",
                principalSchema: "dbo",
                principalTable: "StudioLocations",
                principalColumn: "StudioLocationId");

            migrationBuilder.Sql("""
                -- 空白資料庫沒有需要搬移的舊場次，不必預先建立固定教室。
                -- 有舊場次時仍完整核對教室，避免搬移到錯誤地點。
                IF EXISTS (SELECT 1 FROM dbo.ClassSessions)
                AND NOT EXISTS (
                    SELECT 1
                    FROM dbo.StudioLocations
                    WHERE StudioLocationId = 1
                      AND Name = N'Enjoy Sport Space 享運動空間'
                      AND Address = N'104臺北市中山區撫順街16號2樓'
                )
                BEGIN
                    THROW 50001, N'正式地點資料不符，停止搬移。', 1;
                END;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.ClassSessions
                    WHERE LocationId NOT IN (1, 2)
                )
                BEGIN
                    THROW 50002, N'發現尚未確認對應的舊地點，停止搬移。', 1;
                END;

                UPDATE dbo.ClassSessions
                SET StudioLocationId = 1
                WHERE LocationId IN (1, 2);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_Locations",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_StudioLocationId",
                schema: "dbo",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "StudioLocationId",
                schema: "dbo",
                table: "ClassSessions");
        }
    }
}
