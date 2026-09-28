using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    public partial class ChangeTeacherIdToBigInt : Migration
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

                ALTER TABLE dbo.ClassSessions
                    DROP CONSTRAINT FK_ClassSessions_Teachers_TeacherId;

                DROP INDEX IX_ClassSessions_TeacherId
                    ON dbo.ClassSessions;

                DROP INDEX UX_Teachers_TeacherNo
                    ON dbo.Teachers;

                ALTER TABLE dbo.Teachers
                    DROP CONSTRAINT PK_Teachers;

                ALTER TABLE dbo.Teachers
                    ALTER COLUMN TeacherId bigint NOT NULL;

                ALTER TABLE dbo.ClassSessions
                    ALTER COLUMN TeacherId bigint NOT NULL;

                ALTER TABLE dbo.Teachers
                    ADD CONSTRAINT PK_Teachers
                    PRIMARY KEY (TeacherId);

                CREATE UNIQUE INDEX UX_Teachers_TeacherNo
                    ON dbo.Teachers (TeacherNo)
                    WHERE TeacherNo IS NOT NULL;

                CREATE INDEX IX_ClassSessions_TeacherId
                    ON dbo.ClassSessions (TeacherId);

                ALTER TABLE dbo.ClassSessions WITH CHECK
                    ADD CONSTRAINT FK_ClassSessions_Teachers_TeacherId
                    FOREIGN KEY (TeacherId)
                    REFERENCES dbo.Teachers (TeacherId)
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

                IF EXISTS (
                    SELECT 1
                    FROM dbo.Teachers
                    WHERE TeacherId < -2147483648
                       OR TeacherId > 2147483647
                )
                OR EXISTS (
                    SELECT 1
                    FROM dbo.ClassSessions
                    WHERE TeacherId < -2147483648
                       OR TeacherId > 2147483647
                )
                OR IDENT_CURRENT(N'dbo.Teachers') > 2147483647
                BEGIN
                    THROW 50001,
                        N'TeacherId 或自動編號已超過 int 範圍，無法回復。',
                        1;
                END;

                ALTER TABLE dbo.ClassSessions
                    DROP CONSTRAINT FK_ClassSessions_Teachers_TeacherId;

                DROP INDEX IX_ClassSessions_TeacherId
                    ON dbo.ClassSessions;

                DROP INDEX UX_Teachers_TeacherNo
                    ON dbo.Teachers;

                ALTER TABLE dbo.Teachers
                    DROP CONSTRAINT PK_Teachers;

                ALTER TABLE dbo.Teachers
                    ALTER COLUMN TeacherId int NOT NULL;

                ALTER TABLE dbo.ClassSessions
                    ALTER COLUMN TeacherId int NOT NULL;

                ALTER TABLE dbo.Teachers
                    ADD CONSTRAINT PK_Teachers
                    PRIMARY KEY (TeacherId);

                CREATE UNIQUE INDEX UX_Teachers_TeacherNo
                    ON dbo.Teachers (TeacherNo)
                    WHERE TeacherNo IS NOT NULL;

                CREATE INDEX IX_ClassSessions_TeacherId
                    ON dbo.ClassSessions (TeacherId);

                ALTER TABLE dbo.ClassSessions WITH CHECK
                    ADD CONSTRAINT FK_ClassSessions_Teachers_TeacherId
                    FOREIGN KEY (TeacherId)
                    REFERENCES dbo.Teachers (TeacherId)
                    ON DELETE NO ACTION;
                """);
        }
    }
}