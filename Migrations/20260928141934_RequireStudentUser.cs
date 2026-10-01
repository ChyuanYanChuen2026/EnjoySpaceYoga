using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    public partial class RequireStudentUser : Migration
    {
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

                IF EXISTS (
                    SELECT 1
                    FROM dbo.Students
                    WHERE UserId IS NULL
                )
                BEGIN
                    THROW 50001, N'仍有學生尚未對應 User，停止修改。', 1;
                END;

                ALTER TABLE dbo.Students
                    DROP CONSTRAINT FK_Students_Users;

                DROP INDEX UX_Students_UserId_Transition
                    ON dbo.Students;

                DROP INDEX UX_Students_StudentNo
                    ON dbo.Students;

                ALTER TABLE dbo.Students
                    ALTER COLUMN UserId bigint NOT NULL;

                ALTER TABLE dbo.Students
                    ADD CONSTRAINT UQ_Students_UserId
                    UNIQUE (UserId);

                CREATE UNIQUE INDEX UX_Students_StudentNo
                    ON dbo.Students (StudentNo)
                    WHERE StudentNo IS NOT NULL;

                ALTER TABLE dbo.Students WITH CHECK
                    ADD CONSTRAINT FK_Students_Users
                    FOREIGN KEY (UserId)
                    REFERENCES dbo.Users (UserId)
                    ON DELETE NO ACTION;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                SET ANSI_NULLS ON;
                SET ANSI_PADDING ON;
                SET ANSI_WARNINGS ON;
                SET ARITHABORT ON;
                SET CONCAT_NULL_YIELDS_NULL ON;
                SET QUOTED_IDENTIFIER ON;
                SET NUMERIC_ROUNDABORT OFF;

                ALTER TABLE dbo.Students
                    DROP CONSTRAINT FK_Students_Users;

                ALTER TABLE dbo.Students
                    DROP CONSTRAINT UQ_Students_UserId;

                DROP INDEX UX_Students_StudentNo
                    ON dbo.Students;

                ALTER TABLE dbo.Students
                    ALTER COLUMN UserId bigint NULL;

                CREATE UNIQUE INDEX UX_Students_UserId_Transition
                    ON dbo.Students (UserId)
                    WHERE UserId IS NOT NULL;

                CREATE UNIQUE INDEX UX_Students_StudentNo
                    ON dbo.Students (StudentNo)
                    WHERE StudentNo IS NOT NULL;

                ALTER TABLE dbo.Students WITH CHECK
                    ADD CONSTRAINT FK_Students_Users
                    FOREIGN KEY (UserId)
                    REFERENCES dbo.Users (UserId)
                    ON DELETE NO ACTION;
                """);
        }
    }
}