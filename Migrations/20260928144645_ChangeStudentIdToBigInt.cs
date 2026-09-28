using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    public partial class ChangeStudentIdToBigInt : Migration
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

                ALTER TABLE dbo.EnrollmentApplications
                    DROP CONSTRAINT FK_EnrollmentApplications_Students_StudentId;

                DROP INDEX IX_EnrollmentApplications_StudentId
                    ON dbo.EnrollmentApplications;

                DROP INDEX UX_Students_StudentNo
                    ON dbo.Students;

                ALTER TABLE dbo.Students
                    DROP CONSTRAINT PK_Students;

                ALTER TABLE dbo.Students
                    ALTER COLUMN StudentId bigint NOT NULL;

                ALTER TABLE dbo.EnrollmentApplications
                    ALTER COLUMN StudentId bigint NOT NULL;

                ALTER TABLE dbo.Students
                    ADD CONSTRAINT PK_Students
                    PRIMARY KEY (StudentId);

                CREATE UNIQUE INDEX UX_Students_StudentNo
                    ON dbo.Students (StudentNo)
                    WHERE StudentNo IS NOT NULL;

                CREATE INDEX IX_EnrollmentApplications_StudentId
                    ON dbo.EnrollmentApplications (StudentId);

                ALTER TABLE dbo.EnrollmentApplications WITH CHECK
                    ADD CONSTRAINT FK_EnrollmentApplications_Students_StudentId
                    FOREIGN KEY (StudentId)
                    REFERENCES dbo.Students (StudentId)
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
                    FROM dbo.Students
                    WHERE StudentId < -2147483648
                       OR StudentId > 2147483647
                )
                OR EXISTS (
                    SELECT 1
                    FROM dbo.EnrollmentApplications
                    WHERE StudentId < -2147483648
                       OR StudentId > 2147483647
                )
                OR IDENT_CURRENT(N'dbo.Students') > 2147483647
                BEGIN
                    THROW 50001,
                        N'StudentId 或自動編號已超過 int 範圍，無法回復。',
                        1;
                END;

                ALTER TABLE dbo.EnrollmentApplications
                    DROP CONSTRAINT FK_EnrollmentApplications_Students_StudentId;

                DROP INDEX IX_EnrollmentApplications_StudentId
                    ON dbo.EnrollmentApplications;

                DROP INDEX UX_Students_StudentNo
                    ON dbo.Students;

                ALTER TABLE dbo.Students
                    DROP CONSTRAINT PK_Students;

                ALTER TABLE dbo.Students
                    ALTER COLUMN StudentId int NOT NULL;

                ALTER TABLE dbo.EnrollmentApplications
                    ALTER COLUMN StudentId int NOT NULL;

                ALTER TABLE dbo.Students
                    ADD CONSTRAINT PK_Students
                    PRIMARY KEY (StudentId);

                CREATE UNIQUE INDEX UX_Students_StudentNo
                    ON dbo.Students (StudentNo)
                    WHERE StudentNo IS NOT NULL;

                CREATE INDEX IX_EnrollmentApplications_StudentId
                    ON dbo.EnrollmentApplications (StudentId);

                ALTER TABLE dbo.EnrollmentApplications WITH CHECK
                    ADD CONSTRAINT FK_EnrollmentApplications_Students_StudentId
                    FOREIGN KEY (StudentId)
                    REFERENCES dbo.Students (StudentId)
                    ON DELETE NO ACTION;
                """);
        }
    }
}