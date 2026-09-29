using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class RequireCourseClassCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE c
                SET c.ClassCategoryId = category.ClassCategoryId
                FROM dbo.Courses AS c
                INNER JOIN dbo.Terms AS t
                    ON t.TermId = c.TermId
                INNER JOIN (
                    VALUES
                        (1, N'週一晚間瑜珈班', N'2026 秋季', N'Corporate'),
                        (2, N'週三晚間瑜珈班', N'2026 冬季', N'General'),
                        (3, N'週四晚間瑜珈班', N'2026 秋季', N'General'),
                        (4, N'週四晚間瑜珈班', N'2026 冬季', N'General')
                ) AS mapping (CourseId, CourseName, TermName, CategoryCode)
                    ON c.Id = mapping.CourseId
                    AND c.Name = mapping.CourseName
                    AND t.Name = mapping.TermName
                INNER JOIN dbo.ClassCategories AS category
                    ON category.Code = mapping.CategoryCode
                    AND category.IsActive = 1
                WHERE c.ClassCategoryId IS NULL;

                IF EXISTS (
                    SELECT 1
                    FROM dbo.Courses
                    WHERE ClassCategoryId IS NULL
                )
                BEGIN
                    THROW 50001, N'仍有課程尚未設定分類，停止修改。', 1;
                END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ClassCategoryId",
                schema: "dbo",
                table: "Courses",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ClassCategoryId",
                schema: "dbo",
                table: "Courses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
