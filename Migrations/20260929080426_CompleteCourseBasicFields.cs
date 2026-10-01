using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    /// <inheritdoc />
    public partial class CompleteCourseBasicFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Courses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<bool>(
                name: "AllowsFlexibleBooking",
                schema: "dbo",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_AllowsFlexibleBooking");

            migrationBuilder.AddColumn<bool>(
                name: "AllowsMakeup",
                schema: "dbo",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_AllowsMakeup");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                schema: "dbo",
                table: "Courses",
                type: "datetime2(0)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_CreatedAtUtc");

            migrationBuilder.AddColumn<short>(
                name: "DefaultCapacity",
                schema: "dbo",
                table: "Courses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)10)
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_DefaultCapacity");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_IsActive");

            migrationBuilder.AddColumn<bool>(
                name: "IsPubliclyVisible",
                schema: "dbo",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_IsPubliclyVisible");

            migrationBuilder.AddColumn<decimal>(
                name: "TermPrice",
                schema: "dbo",
                table: "Courses",
                type: "decimal(10,2)",
                nullable: true);

            // 一次性補價：冬季沿用同類秋季 Demo 價格；未標示班別的測試課程依分類補價。
            migrationBuilder.Sql("""
                UPDATE c
                SET TermPrice = CASE
                    WHEN c.Name LIKE N'%週六%' OR c.Name LIKE N'%TRX%' THEN 3280
                    WHEN c.Name LIKE N'%週一%' THEN 2460
                    WHEN c.Name LIKE N'%週三%' OR c.Name LIKE N'%週四%' THEN 3690
                    WHEN category.Code = N'Corporate' THEN 2460
                    WHEN category.Code = N'General' THEN 3690
                    ELSE NULL
                END
                FROM dbo.Courses AS c
                INNER JOIN dbo.ClassCategories AS category
                    ON category.ClassCategoryId = c.ClassCategoryId;

                IF EXISTS (SELECT 1 FROM dbo.Courses WHERE TermPrice IS NULL)
                    THROW 50001, N'有課程無法判定 Demo 期課價格，請先確認。', 1;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "TermPrice", schema: "dbo", table: "Courses",
                type: "decimal(10,2)", nullable: false,
                oldClrType: typeof(decimal), oldType: "decimal(10,2)", oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_DefaultCapacity", schema: "dbo", table: "Courses",
                sql: "[DefaultCapacity] > 0");
            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_TermPrice", schema: "dbo", table: "Courses",
                sql: "[TermPrice] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM dbo.Courses WHERE DATALENGTH(Name) > 200)
                    THROW 50001, N'課程名稱超過 100 字，無法回復舊欄位長度。', 1;
                """);
            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_DefaultCapacity", schema: "dbo", table: "Courses");
            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_TermPrice", schema: "dbo", table: "Courses");
            migrationBuilder.DropColumn(
                name: "AllowsFlexibleBooking",
                schema: "dbo",
                table: "Courses")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_AllowsFlexibleBooking");

            migrationBuilder.DropColumn(
                name: "AllowsMakeup",
                schema: "dbo",
                table: "Courses")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_AllowsMakeup");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                schema: "dbo",
                table: "Courses")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_CreatedAtUtc");

            migrationBuilder.DropColumn(
                name: "DefaultCapacity",
                schema: "dbo",
                table: "Courses")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_DefaultCapacity");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Courses")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_IsActive");

            migrationBuilder.DropColumn(
                name: "IsPubliclyVisible",
                schema: "dbo",
                table: "Courses")
                .Annotation("Relational:DefaultConstraintName", "DF_Courses_IsPubliclyVisible");

            migrationBuilder.DropColumn(
                name: "TermPrice",
                schema: "dbo",
                table: "Courses");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Courses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
