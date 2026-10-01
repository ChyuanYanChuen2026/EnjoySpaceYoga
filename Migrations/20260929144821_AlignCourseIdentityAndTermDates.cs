using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnjoySpace.Web.Migrations
{
    public partial class AlignCourseIdentityAndTermDates : Migration
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
                    DROP CONSTRAINT FK_ClassSessions_Courses_CourseId;

                ALTER TABLE dbo.EnrollmentApplications
                    DROP CONSTRAINT FK_EnrollmentApplications_Courses_CourseId;

                DROP INDEX IX_ClassSessions_CourseId
                    ON dbo.ClassSessions;

                DROP INDEX IX_EnrollmentApplications_CourseId
                    ON dbo.EnrollmentApplications;

                ALTER TABLE dbo.Courses
                    DROP CONSTRAINT PK_Courses;

                EXEC sp_rename N'dbo.Courses.Id', N'CourseId', N'COLUMN';

                ALTER TABLE dbo.Courses
                    ALTER COLUMN CourseId bigint NOT NULL;

                ALTER TABLE dbo.ClassSessions
                    ALTER COLUMN CourseId bigint NOT NULL;

                ALTER TABLE dbo.EnrollmentApplications
                    ALTER COLUMN CourseId bigint NOT NULL;

                ALTER TABLE dbo.Courses
                    DROP COLUMN StartDate, EndDate;

                ALTER TABLE dbo.Courses
                    ADD CONSTRAINT PK_Courses
                    PRIMARY KEY (CourseId);

                CREATE INDEX IX_ClassSessions_CourseId
                    ON dbo.ClassSessions (CourseId);

                CREATE INDEX IX_EnrollmentApplications_CourseId
                    ON dbo.EnrollmentApplications (CourseId);

                ALTER TABLE dbo.ClassSessions WITH CHECK
                    ADD CONSTRAINT FK_ClassSessions_Courses_CourseId
                    FOREIGN KEY (CourseId)
                    REFERENCES dbo.Courses (CourseId)
                    ON DELETE NO ACTION;

                ALTER TABLE dbo.EnrollmentApplications WITH CHECK
                    ADD CONSTRAINT FK_EnrollmentApplications_Courses_CourseId
                    FOREIGN KEY (CourseId)
                    REFERENCES dbo.Courses (CourseId)
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
                    SELECT 1 FROM dbo.Courses
                    WHERE CourseId < -2147483648 OR CourseId > 2147483647
                )
                OR EXISTS (
                    SELECT 1 FROM dbo.ClassSessions
                    WHERE CourseId < -2147483648 OR CourseId > 2147483647
                )
                OR EXISTS (
                    SELECT 1 FROM dbo.EnrollmentApplications
                    WHERE CourseId < -2147483648 OR CourseId > 2147483647
                )
                OR IDENT_CURRENT(N'dbo.Courses') > 2147483647
                BEGIN
                    THROW 50001,
                        N'CourseId 或自動編號已超過 int 範圍，無法回復。',
                        1;
                END;

                ALTER TABLE dbo.ClassSessions
                    DROP CONSTRAINT FK_ClassSessions_Courses_CourseId;

                ALTER TABLE dbo.EnrollmentApplications
                    DROP CONSTRAINT FK_EnrollmentApplications_Courses_CourseId;

                DROP INDEX IX_ClassSessions_CourseId
                    ON dbo.ClassSessions;

                DROP INDEX IX_EnrollmentApplications_CourseId
                    ON dbo.EnrollmentApplications;

                ALTER TABLE dbo.Courses
                    DROP CONSTRAINT PK_Courses;

                ALTER TABLE dbo.Courses
                    ALTER COLUMN CourseId int NOT NULL;

                ALTER TABLE dbo.ClassSessions
                    ALTER COLUMN CourseId int NOT NULL;

                ALTER TABLE dbo.EnrollmentApplications
                    ALTER COLUMN CourseId int NOT NULL;

                ALTER TABLE dbo.Courses
                    ADD StartDate date NULL,
                        EndDate date NULL;

                UPDATE course
                SET StartDate = term.StartDate,
                    EndDate = term.EndDate
                FROM dbo.Courses course
                INNER JOIN dbo.Terms term ON term.TermId = course.TermId;

                IF EXISTS (
                    SELECT 1 FROM dbo.Courses
                    WHERE StartDate IS NULL OR EndDate IS NULL
                )
                    THROW 50002, N'有課程無法由學期還原日期，停止回復。', 1;

                ALTER TABLE dbo.Courses ALTER COLUMN StartDate date NOT NULL;
                ALTER TABLE dbo.Courses ALTER COLUMN EndDate date NOT NULL;

                ALTER TABLE dbo.Courses
                    ADD CONSTRAINT PK_Courses
                    PRIMARY KEY (CourseId);

                EXEC sp_rename N'dbo.Courses.CourseId', N'Id', N'COLUMN';

                CREATE INDEX IX_ClassSessions_CourseId
                    ON dbo.ClassSessions (CourseId);

                CREATE INDEX IX_EnrollmentApplications_CourseId
                    ON dbo.EnrollmentApplications (CourseId);

                ALTER TABLE dbo.ClassSessions WITH CHECK
                    ADD CONSTRAINT FK_ClassSessions_Courses_CourseId
                    FOREIGN KEY (CourseId)
                    REFERENCES dbo.Courses (Id)
                    ON DELETE NO ACTION;

                ALTER TABLE dbo.EnrollmentApplications WITH CHECK
                    ADD CONSTRAINT FK_EnrollmentApplications_Courses_CourseId
                    FOREIGN KEY (CourseId)
                    REFERENCES dbo.Courses (Id)
                    ON DELETE NO ACTION;
                """);
        }
    }
}
