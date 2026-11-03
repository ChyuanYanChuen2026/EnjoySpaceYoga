SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
IF @@TRANCOUNT=0 THROW 51090, N'Recovery requires an EF migration transaction.',1;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'EnjoySpace.ReconcileRecoveredSchema', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000;
IF @lockResult<0 THROW 51090,N'Cannot acquire recovery schema lock.',1;
DECLARE @Documents table (Label char(1) PRIMARY KEY, Document nvarchar(max));
CREATE TABLE #RecoveryRows (Label char(1), Kind nvarchar(32), ObjectKey nvarchar(512) COLLATE Latin1_General_100_BIN2, Signature varbinary(32));
INSERT @Documents VALUES ('L',N'{"databaseName":"EnjoySpace_RecoveryChecks_05de4301fcb84893b5814360df33f633","capturedAtUtc":"2026-11-02T03:01:32.6217997","migrations":[{"MigrationId":"20260920144316_InitialCreate","ProductVersion":"10.0.14"},{"MigrationId":"20260921153358_AddCourseRowVersion","ProductVersion":"10.0.14"},{"MigrationId":"20260921163921_AddTeachersAndLocations","ProductVersion":"10.0.14"},{"MigrationId":"20260921165340_AddClassSessions","ProductVersion":"10.0.14"},{"MigrationId":"20260922123750_AddClassSessionChecks","ProductVersion":"10.0.14"},{"MigrationId":"20260923053339_AddStudents","ProductVersion":"10.0.14"},{"MigrationId":"20260923062750_AddEnrollmentApplications","ProductVersion":"10.0.14"},{"MigrationId":"20260923075505_AddEnrollmentApplicationStatusCheck","ProductVersion":"10.0.14"},{"MigrationId":"20260925073616_AddUsers","ProductVersion":"10.0.14"},{"MigrationId":"20260925084606_AddRoles","ProductVersion":"10.0.14"},{"MigrationId":"20260926081020_AddUserRoles","ProductVersion":"10.0.14"},{"MigrationId":"20260928053810_AddTerms","ProductVersion":"10.0.14"},{"MigrationId":"20260928054501_AddClassCategories","ProductVersion":"10.0.14"},{"MigrationId":"20260928064739_AddStudioLocations","ProductVersion":"10.0.14"},{"MigrationId":"20260928075158_LinkSessionsToStudioLocations","ProductVersion":"10.0.14"},{"MigrationId":"20260928081846_SwitchSessionsToStudioLocations","ProductVersion":"10.0.14"},{"MigrationId":"20260928091511_RemoveLegacyLocations","ProductVersion":"10.0.14"},{"MigrationId":"20260928092342_AddTeacherUserLink","ProductVersion":"10.0.14"},{"MigrationId":"20260928095018_AddTeacherProfileFields","ProductVersion":"10.0.14"},{"MigrationId":"20260928102121_RequireTeacherUser","ProductVersion":"10.0.14"},{"MigrationId":"20260928114431_RemoveLegacyTeacherName","ProductVersion":"10.0.14"},{"MigrationId":"20260928115922_RemoveLegacyTeacherRowVersion","ProductVersion":"10.0.14"},{"MigrationId":"20260928124730_RenameTeacherPrimaryKey","ProductVersion":"10.0.14"},{"MigrationId":"20260928125635_ChangeTeacherIdToBigInt","ProductVersion":"10.0.14"},{"MigrationId":"20260928134630_AddStudentUserLink","ProductVersion":"10.0.14"},{"MigrationId":"20260928140921_AddStudentProfileFields","ProductVersion":"10.0.14"},{"MigrationId":"20260928141934_RequireStudentUser","ProductVersion":"10.0.14"},{"MigrationId":"20260928143153_RemoveLegacyStudentContactFields","ProductVersion":"10.0.14"},{"MigrationId":"20260928143735_RemoveLegacyStudentRowVersion","ProductVersion":"10.0.14"},{"MigrationId":"20260928144059_RenameStudentPrimaryKey","ProductVersion":"10.0.14"},{"MigrationId":"20260928144645_ChangeStudentIdToBigInt","ProductVersion":"10.0.14"},{"MigrationId":"20260928145601_AddCourseTermLink","ProductVersion":"10.0.14"},{"MigrationId":"20260929015158_RequireCourseTerm","ProductVersion":"10.0.14"},{"MigrationId":"20260929022545_AddCourseClassCategoryLink","ProductVersion":"10.0.14"},{"MigrationId":"20260929040912_RequireCourseClassCategory","ProductVersion":"10.0.14"},{"MigrationId":"20260929055002_AddCourseDefaultTeacherLink","ProductVersion":"10.0.14"},{"MigrationId":"20260929064929_RequireCourseDefaultTeacher","ProductVersion":"10.0.14"},{"MigrationId":"20260929065658_AddCourseDefaultLocationLink","ProductVersion":"10.0.14"},{"MigrationId":"20260929073952_RequireCourseDefaultLocation","ProductVersion":"10.0.14"},{"MigrationId":"20260929080426_CompleteCourseBasicFields","ProductVersion":"10.0.14"},{"MigrationId":"20260929144821_AlignCourseIdentityAndTermDates","ProductVersion":"10.0.14"},{"MigrationId":"20260930034905_AddEnrollmentPaymentWorkflow","ProductVersion":"10.0.14"},{"MigrationId":"20261006144721_ScheduleConfirmationAndEnrollmentConsent","ProductVersion":"10.0.14"},{"MigrationId":"20261008072606_AddSessionParticipantsAndAttendance","ProductVersion":"10.0.14"},{"MigrationId":"20261011123639_AddLeaveAndMakeupWorkflow","ProductVersion":"10.0.14"},{"MigrationId":"20261011131928_AddTrialPlansAndPurchases","ProductVersion":"10.0.14"},{"MigrationId":"20261011174223_AddPortalTrialRequests","ProductVersion":"10.0.14"},{"MigrationId":"20261011183856_AddTeacherRequests","ProductVersion":"10.0.14"},{"MigrationId":"20261016081127_AddPortalTrialReferrals","ProductVersion":"10.0.14"},{"MigrationId":"20261016081308_SupportMonthlyLessonPlans","ProductVersion":"10.0.14"}],"tables":[{"schema":"dbo","table":"__EFMigrationsHistory"},{"schema":"dbo","table":"AttendanceRecords"},{"schema":"dbo","table":"ClassCategories"},{"schema":"dbo","table":"ClassSessionParticipants"},{"schema":"dbo","table":"ClassSessions"},{"schema":"dbo","table":"CourseEnrollments"},{"schema":"dbo","table":"Courses"},{"schema":"dbo","table":"EnrollmentApplications"},{"schema":"dbo","table":"LeaveRequests"},{"schema":"dbo","table":"LessonPasses"},{"schema":"dbo","table":"LessonPlans"},{"schema":"dbo","table":"MakeupEntitlements"},{"schema":"dbo","table":"MakeupRequests"},{"schema":"dbo","table":"Payments"},{"schema":"dbo","table":"PortalTrialRequests"},{"schema":"dbo","table":"Roles"},{"schema":"dbo","table":"Students"},{"schema":"dbo","table":"StudioLocations"},{"schema":"dbo","table":"TeacherRequests"},{"schema":"dbo","table":"Teachers"},{"schema":"dbo","table":"Terms"},{"schema":"dbo","table":"TrialGuests"},{"schema":"dbo","table":"UserRoles"},{"schema":"dbo","table":"Users"}],"columns":[{"schema":"dbo","table":"__EFMigrationsHistory","column":"MigrationId","type":"nvarchar","maxLengthBytes":300,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"__EFMigrationsHistory","column":"ProductVersion","type":"nvarchar","maxLengthBytes":64,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"AttendanceRecordId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"SessionParticipantId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"CheckInMethod","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"CheckedInByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"ManualReason","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"UpdatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"AttendanceRecords","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassCategories","column":"ClassCategoryId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassCategories","column":"Code","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassCategories","column":"Name","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassCategories","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_ClassCategories_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"SessionParticipantId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"ClassSessionId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"CourseEnrollmentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"ParticipationType","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"AddedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"AddedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__ClassSess__Added__4C6B5938","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessionParticipants","column":"MakeupRequestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"Id","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"CourseId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"TeacherId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"StartsAt","type":"datetimeoffset","maxLengthBytes":10,"precision":34,"scale":7,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"EndsAt","type":"datetimeoffset","maxLengthBytes":10,"precision":34,"scale":7,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"Capacity","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"CapacityOverride","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"StudioLocationId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"ClassSessions","column":"IsExtraSession","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__ClassSess__IsExt__00DF2177","defaultSql":"(CONVERT([bit],(0)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"CourseEnrollmentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"CourseId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"EnrollmentApplicationId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"EnrolledAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__CourseEnr__Enrol__32AB8735","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"CancelledAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"CreatedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"CourseEnrollments","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"CourseId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"Name","type":"nvarchar","maxLengthBytes":300,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"TermId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"ClassCategoryId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"DefaultTeacherId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"DefaultLocationId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"AllowsFlexibleBooking","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Courses_AllowsFlexibleBooking","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"AllowsMakeup","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Courses_AllowsMakeup","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Courses_CreatedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"DefaultCapacity","type":"smallint","maxLengthBytes":2,"precision":5,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Courses_DefaultCapacity","defaultSql":"(CONVERT([smallint],(10)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Courses_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"IsPubliclyVisible","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Courses_IsPubliclyVisible","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Courses","column":"TermPrice","type":"decimal","maxLengthBytes":9,"precision":10,"scale":2,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"EnrollmentApplicationId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"CourseId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"AppliedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"ReviewedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"ReviewNote","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"ApplicationType","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":"DF__Enrollmen__Appli__2B0A656D","defaultSql":"(N''FullTerm'')","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"FirstSessionId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"PaymentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"QuotedAmount","type":"decimal","maxLengthBytes":9,"precision":10,"scale":2,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__Enrollmen__Quote__2BFE89A6","defaultSql":"((0.0))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"QuotedSessionCount","type":"smallint","maxLengthBytes":2,"precision":5,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__Enrollmen__Quote__2CF2ADDF","defaultSql":"(CONVERT([smallint],(0)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"ReviewedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"ConfirmedPolicy","type":"nvarchar","maxLengthBytes":400,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"PolicyConfirmedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"EnrollmentApplications","column":"PolicyConfirmedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"LeaveRequestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"SessionParticipantId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"RequestSource","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"Reason","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"CreatedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"RequestedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"CancelledByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"CancelledAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LeaveRequests","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"LessonPassId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"PurchaseKey","type":"uniqueidentifier","maxLengthBytes":16,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"LessonPlanId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"TrialGuestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"PaymentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"PlanNameAtPurchase","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"PlanTypeAtPurchase","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"SessionQuotaAtPurchase","type":"smallint","maxLengthBytes":2,"precision":5,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"AmountAtPurchase","type":"decimal","maxLengthBytes":9,"precision":10,"scale":2,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"ValidFromDate","type":"date","maxLengthBytes":3,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"ValidUntilDate","type":"date","maxLengthBytes":3,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"PurchasedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"CreatedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"Note","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPasses","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"LessonPlanId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"PlanCode","type":"nvarchar","maxLengthBytes":100,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"PlanName","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"PlanType","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"AudienceType","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"SessionQuota","type":"smallint","maxLengthBytes":2,"precision":5,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"Price","type":"decimal","maxLengthBytes":9,"precision":10,"scale":2,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"ValidityUnit","type":"nvarchar","maxLengthBytes":40,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"ValidityValue","type":"smallint","maxLengthBytes":2,"precision":5,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"LessonPlans","column":"RequiresReferrer","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__LessonPla__Requi__0E391C95","defaultSql":"(CONVERT([bit],(0)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"MakeupEntitlementId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"SourceTermId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"SourceLeaveRequestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"SourceType","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"ConsumedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"RevokedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupEntitlements","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"MakeupRequestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"MakeupEntitlementId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"TargetClassSessionId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"RequestedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"RequestedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"ReviewedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"ReviewedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"ReviewNote","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"CancelledByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"CancelledAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"CancellationReason","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"MakeupRequests","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"PaymentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"TrialGuestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"PaymentPurpose","type":"nvarchar","maxLengthBytes":100,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"Amount","type":"decimal","maxLengthBytes":9,"precision":10,"scale":2,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"PaymentMethod","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"PaidAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"ConfirmedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"ConfirmedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"Note","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Payments","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__Payments__Create__3F115E1A","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"Id","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"SubmissionKey","type":"uniqueidentifier","maxLengthBytes":16,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"UserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"TrialGuestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"ClassSessionId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"LessonPlanId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"PlanName","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"QuotedPrice","type":"decimal","maxLengthBytes":9,"precision":10,"scale":2,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"Note","type":"nvarchar","maxLengthBytes":600,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":8,"precision":27,"scale":7,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"PortalTrialRequests","column":"ReferrerName","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Roles","column":"RoleId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Roles","column":"RoleCode","type":"nvarchar","maxLengthBytes":100,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Roles","column":"RoleName","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Roles","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Roles_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"StudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"UserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Students_CreatedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Students_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"JoinedDate","type":"date","maxLengthBytes":3,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"Note","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Students","column":"StudentNo","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"StudioLocationId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"Name","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"Address","type":"nvarchar","maxLengthBytes":600,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"Latitude","type":"decimal","maxLengthBytes":5,"precision":9,"scale":6,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"Longitude","type":"decimal","maxLengthBytes":5,"precision":9,"scale":6,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"CheckInRadiusMeters","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_StudioLocations_Radius","defaultSql":"((500))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_StudioLocations_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"StudioLocations","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_StudioLocations_CreatedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"Id","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"SubmissionKey","type":"uniqueidentifier","maxLengthBytes":16,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"Kind","type":"nvarchar","maxLengthBytes":40,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"TeacherId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"CourseId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"TargetSessionId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"CreatedSessionId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"StudioLocationId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"StartsAt","type":"datetimeoffset","maxLengthBytes":10,"precision":34,"scale":7,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"EndsAt","type":"datetimeoffset","maxLengthBytes":10,"precision":34,"scale":7,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"RequestedCapacity","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"OriginalCapacity","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"TargetVersion","type":"varbinary","maxLengthBytes":8,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"Reason","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"Status","type":"nvarchar","maxLengthBytes":40,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"SubmittedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"SubmittedAt","type":"datetimeoffset","maxLengthBytes":10,"precision":34,"scale":7,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"ReviewedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"ReviewedAt","type":"datetimeoffset","maxLengthBytes":10,"precision":34,"scale":7,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"ReviewNote","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TeacherRequests","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Teachers","column":"TeacherId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Teachers","column":"UserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Teachers","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Teachers_CreatedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Teachers","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Teachers_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Teachers","column":"Note","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Teachers","column":"TeacherNo","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"TermId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"Name","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"StartDate","type":"date","maxLengthBytes":3,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"EndDate","type":"date","maxLengthBytes":3,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"RegistrationStartAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"RegistrationEndAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":"DF_Terms_Status","defaultSql":"(N''Draft'')","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Terms_CreatedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"ScheduleConfirmedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Terms","column":"ScheduleConfirmedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"TrialGuestId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"Name","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"Phone","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"Email","type":"nvarchar","maxLengthBytes":400,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"LineId","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"SourceType","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"ReferrerStudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"ConvertedStudentId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"Status","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"Note","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"CreatedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF__TrialGues__Creat__395884C4","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"UpdatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"TrialGuests","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"UserRoles","column":"UserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"UserRoles","column":"RoleId","type":"int","maxLengthBytes":4,"precision":10,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"UserRoles","column":"AssignedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_UserRoles_AssignedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"UserRoles","column":"AssignedByUserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"UserId","type":"bigint","maxLengthBytes":8,"precision":19,"scale":0,"nullable":false,"identity":true,"identitySeed":"1","identityIncrement":"1","collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"Account","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"PasswordHash","type":"nvarchar","maxLengthBytes":1000,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"DisplayName","type":"nvarchar","maxLengthBytes":200,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"Email","type":"nvarchar","maxLengthBytes":400,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"Phone","type":"nvarchar","maxLengthBytes":60,"precision":0,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":"Chinese_Taiwan_Stroke_CI_AS","defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"IsActive","type":"bit","maxLengthBytes":1,"precision":1,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Users_IsActive","defaultSql":"(CONVERT([bit],(1)))","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"CreatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":"DF_Users_CreatedAtUtc","defaultSql":"(sysutcdatetime())","computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"UpdatedAtUtc","type":"datetime2","maxLengthBytes":6,"precision":19,"scale":0,"nullable":true,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null},{"schema":"dbo","table":"Users","column":"RowVersion","type":"timestamp","maxLengthBytes":8,"precision":0,"scale":0,"nullable":false,"identity":false,"identitySeed":null,"identityIncrement":null,"collation":null,"defaultName":null,"defaultSql":null,"computedSql":null,"computedPersisted":null}],"indexes":[{"schema":"dbo","table":"__EFMigrationsHistory","name":"PK___EFMigrationsHistory","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"MigrationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"AttendanceRecords","name":"IX_AttendanceRecords_CheckedInByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CheckedInByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"AttendanceRecords","name":"IX_AttendanceRecords_SessionParticipantId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SessionParticipantId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"AttendanceRecords","name":"IX_AttendanceRecords_Status","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Status","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"AttendanceRecords","name":"PK_AttendanceRecords","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"AttendanceRecordId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassCategories","name":"PK_ClassCategories","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ClassCategoryId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassCategories","name":"UQ_ClassCategories_Code","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"Code","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"IX_ClassSessionParticipants_AddedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"AddedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([ParticipationType]=N''Regular'')","disabled":false,"columns":[{"column":"ClassSessionId","ordinal":1,"descending":false,"included":false},{"column":"CourseEnrollmentId","ordinal":2,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"IX_ClassSessionParticipants_CourseEnrollmentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CourseEnrollmentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"IX_ClassSessionParticipants_MakeupRequestId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([MakeupRequestId] IS NOT NULL)","disabled":false,"columns":[{"column":"MakeupRequestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"IX_ClassSessionParticipants_StudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"PK_ClassSessionParticipants","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SessionParticipantId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"UX_ClassSessionParticipants_ActiveStudent","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([Status]=N''Active'')","disabled":false,"columns":[{"column":"ClassSessionId","ordinal":1,"descending":false,"included":false},{"column":"StudentId","ordinal":2,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessions","name":"IX_ClassSessions_CourseId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CourseId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessions","name":"IX_ClassSessions_StudioLocationId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudioLocationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessions","name":"IX_ClassSessions_TeacherId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TeacherId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"ClassSessions","name":"PK_ClassSessions","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Id","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"CourseEnrollments","name":"IX_CourseEnrollments_CreatedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CreatedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"CourseEnrollments","name":"IX_CourseEnrollments_StudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"CourseEnrollments","name":"PK_CourseEnrollments","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CourseEnrollmentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"CourseEnrollments","name":"UQ_CourseEnrollments_Application","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"EnrollmentApplicationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"CourseEnrollments","name":"UQ_CourseEnrollments_Course_Student","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"CourseId","ordinal":1,"descending":false,"included":false},{"column":"StudentId","ordinal":2,"descending":false,"included":false}]},{"schema":"dbo","table":"Courses","name":"IX_Courses_ClassCategoryId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ClassCategoryId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Courses","name":"IX_Courses_DefaultLocationId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"DefaultLocationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Courses","name":"IX_Courses_DefaultTeacherId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"DefaultTeacherId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Courses","name":"IX_Courses_TermId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TermId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Courses","name":"PK_Courses","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CourseId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"IX_EnrollmentApplications_CourseId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CourseId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"IX_EnrollmentApplications_FirstSessionId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"FirstSessionId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"IX_EnrollmentApplications_PaymentId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([PaymentId] IS NOT NULL)","disabled":false,"columns":[{"column":"PaymentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"IX_EnrollmentApplications_ReviewedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ReviewedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"IX_EnrollmentApplications_Status","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Status","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"IX_EnrollmentApplications_StudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"EnrollmentApplications","name":"PK_EnrollmentApplications","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"EnrollmentApplicationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LeaveRequests","name":"IX_LeaveRequests_CancelledByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CancelledByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LeaveRequests","name":"IX_LeaveRequests_CreatedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CreatedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LeaveRequests","name":"IX_LeaveRequests_SessionParticipantId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([Status]=N''Active'')","disabled":false,"columns":[{"column":"SessionParticipantId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LeaveRequests","name":"PK_LeaveRequests","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"LeaveRequestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"IX_LessonPasses_CreatedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CreatedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"IX_LessonPasses_LessonPlanId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"LessonPlanId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"IX_LessonPasses_PaymentId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"PaymentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"IX_LessonPasses_PurchaseKey","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"PurchaseKey","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"IX_LessonPasses_StudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"IX_LessonPasses_TrialGuestId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TrialGuestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPasses","name":"PK_LessonPasses","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"LessonPassId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPlans","name":"IX_LessonPlans_PlanCode","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"PlanCode","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"LessonPlans","name":"PK_LessonPlans","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"LessonPlanId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupEntitlements","name":"IX_MakeupEntitlements_SourceLeaveRequestId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SourceLeaveRequestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupEntitlements","name":"IX_MakeupEntitlements_SourceTermId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SourceTermId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupEntitlements","name":"IX_MakeupEntitlements_StudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupEntitlements","name":"PK_MakeupEntitlements","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"MakeupEntitlementId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupRequests","name":"IX_MakeupRequests_CancelledByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CancelledByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupRequests","name":"IX_MakeupRequests_MakeupEntitlementId_Status","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"MakeupEntitlementId","ordinal":1,"descending":false,"included":false},{"column":"Status","ordinal":2,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupRequests","name":"IX_MakeupRequests_RequestedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"RequestedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupRequests","name":"IX_MakeupRequests_ReviewedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ReviewedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupRequests","name":"IX_MakeupRequests_TargetClassSessionId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TargetClassSessionId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"MakeupRequests","name":"PK_MakeupRequests","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"MakeupRequestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Payments","name":"IX_Payments_ConfirmedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ConfirmedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Payments","name":"IX_Payments_Status","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Status","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Payments","name":"IX_Payments_StudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Payments","name":"IX_Payments_TrialGuestId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TrialGuestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Payments","name":"PK_Payments","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"PaymentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"PortalTrialRequests","name":"IX_PortalTrialRequests_ClassSessionId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ClassSessionId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"PortalTrialRequests","name":"IX_PortalTrialRequests_LessonPlanId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"LessonPlanId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"PortalTrialRequests","name":"IX_PortalTrialRequests_SubmissionKey","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SubmissionKey","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"PortalTrialRequests","name":"IX_PortalTrialRequests_TrialGuestId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TrialGuestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"PortalTrialRequests","name":"IX_PortalTrialRequests_UserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"UserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"PortalTrialRequests","name":"PK_PortalTrialRequests","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Id","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Roles","name":"PK_Roles","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"RoleId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Roles","name":"UQ_Roles_RoleCode","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"RoleCode","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Students","name":"PK_Students","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Students","name":"UQ_Students_UserId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"UserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Students","name":"UX_Students_StudentNo","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([StudentNo] IS NOT NULL)","disabled":false,"columns":[{"column":"StudentNo","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"StudioLocations","name":"PK_StudioLocations","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudioLocationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_CourseId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CourseId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_CreatedSessionId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([CreatedSessionId] IS NOT NULL)","disabled":false,"columns":[{"column":"CreatedSessionId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_ReviewedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ReviewedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_StudioLocationId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"StudioLocationId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_SubmissionKey","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SubmissionKey","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_SubmittedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"SubmittedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_TargetSessionId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([Kind]=N''Capacity'' AND [Status]=N''Pending'')","disabled":false,"columns":[{"column":"TargetSessionId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"IX_TeacherRequests_TeacherId_Status","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TeacherId","ordinal":1,"descending":false,"included":false},{"column":"Status","ordinal":2,"descending":false,"included":false}]},{"schema":"dbo","table":"TeacherRequests","name":"PK_TeacherRequests","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Id","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Teachers","name":"PK_Teachers","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TeacherId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Teachers","name":"UQ_Teachers_UserId","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"UserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Teachers","name":"UX_Teachers_TeacherNo","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":"([TeacherNo] IS NOT NULL)","disabled":false,"columns":[{"column":"TeacherNo","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Terms","name":"PK_Terms","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TermId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TrialGuests","name":"IX_TrialGuests_ConvertedStudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ConvertedStudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TrialGuests","name":"IX_TrialGuests_CreatedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"CreatedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TrialGuests","name":"IX_TrialGuests_Email","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Email","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TrialGuests","name":"IX_TrialGuests_Phone","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"Phone","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TrialGuests","name":"IX_TrialGuests_ReferrerStudentId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"ReferrerStudentId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"TrialGuests","name":"PK_TrialGuests","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"TrialGuestId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"UserRoles","name":"IX_UserRoles_AssignedByUserId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"AssignedByUserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"UserRoles","name":"IX_UserRoles_RoleId","type":"NONCLUSTERED","isUnique":false,"isPrimaryKey":false,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"RoleId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"UserRoles","name":"PK_UserRoles","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"UserId","ordinal":1,"descending":false,"included":false},{"column":"RoleId","ordinal":2,"descending":false,"included":false}]},{"schema":"dbo","table":"Users","name":"PK_Users","type":"CLUSTERED","isUnique":true,"isPrimaryKey":true,"isUniqueConstraint":false,"filterSql":null,"disabled":false,"columns":[{"column":"UserId","ordinal":1,"descending":false,"included":false}]},{"schema":"dbo","table":"Users","name":"UQ_Users_Account","type":"NONCLUSTERED","isUnique":true,"isPrimaryKey":false,"isUniqueConstraint":true,"filterSql":null,"disabled":false,"columns":[{"column":"Account","ordinal":1,"descending":false,"included":false}]}],"foreignKeys":[{"schema":"dbo","table":"AttendanceRecords","name":"FK_AttendanceRecords_ClassSessionParticipants_SessionParticipantId","principalSchema":"dbo","principalTable":"ClassSessionParticipants","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"SessionParticipantId","principalColumn":"SessionParticipantId","ordinal":1}]},{"schema":"dbo","table":"AttendanceRecords","name":"FK_AttendanceRecords_Users_CheckedInByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CheckedInByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"FK_ClassSessionParticipants_ClassSessions_ClassSessionId","principalSchema":"dbo","principalTable":"ClassSessions","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ClassSessionId","principalColumn":"Id","ordinal":1}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"FK_ClassSessionParticipants_CourseEnrollments_CourseEnrollmentId","principalSchema":"dbo","principalTable":"CourseEnrollments","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CourseEnrollmentId","principalColumn":"CourseEnrollmentId","ordinal":1}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"FK_ClassSessionParticipants_MakeupRequests_MakeupRequestId","principalSchema":"dbo","principalTable":"MakeupRequests","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"MakeupRequestId","principalColumn":"MakeupRequestId","ordinal":1}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"FK_ClassSessionParticipants_Students_StudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"ClassSessionParticipants","name":"FK_ClassSessionParticipants_Users_AddedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"AddedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"ClassSessions","name":"FK_ClassSessions_Courses_CourseId","principalSchema":"dbo","principalTable":"Courses","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CourseId","principalColumn":"CourseId","ordinal":1}]},{"schema":"dbo","table":"ClassSessions","name":"FK_ClassSessions_Locations","principalSchema":"dbo","principalTable":"StudioLocations","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudioLocationId","principalColumn":"StudioLocationId","ordinal":1}]},{"schema":"dbo","table":"ClassSessions","name":"FK_ClassSessions_Teachers_TeacherId","principalSchema":"dbo","principalTable":"Teachers","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TeacherId","principalColumn":"TeacherId","ordinal":1}]},{"schema":"dbo","table":"CourseEnrollments","name":"FK_CourseEnrollments_Courses_CourseId","principalSchema":"dbo","principalTable":"Courses","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CourseId","principalColumn":"CourseId","ordinal":1}]},{"schema":"dbo","table":"CourseEnrollments","name":"FK_CourseEnrollments_EnrollmentApplications_EnrollmentApplicationId","principalSchema":"dbo","principalTable":"EnrollmentApplications","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"EnrollmentApplicationId","principalColumn":"EnrollmentApplicationId","ordinal":1}]},{"schema":"dbo","table":"CourseEnrollments","name":"FK_CourseEnrollments_Students_StudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"CourseEnrollments","name":"FK_CourseEnrollments_Users_CreatedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CreatedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"Courses","name":"FK_Courses_ClassCategories","principalSchema":"dbo","principalTable":"ClassCategories","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ClassCategoryId","principalColumn":"ClassCategoryId","ordinal":1}]},{"schema":"dbo","table":"Courses","name":"FK_Courses_DefaultLocation","principalSchema":"dbo","principalTable":"StudioLocations","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"DefaultLocationId","principalColumn":"StudioLocationId","ordinal":1}]},{"schema":"dbo","table":"Courses","name":"FK_Courses_DefaultTeacher","principalSchema":"dbo","principalTable":"Teachers","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"DefaultTeacherId","principalColumn":"TeacherId","ordinal":1}]},{"schema":"dbo","table":"Courses","name":"FK_Courses_Terms","principalSchema":"dbo","principalTable":"Terms","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TermId","principalColumn":"TermId","ordinal":1}]},{"schema":"dbo","table":"EnrollmentApplications","name":"FK_EnrollmentApplications_ClassSessions_FirstSessionId","principalSchema":"dbo","principalTable":"ClassSessions","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"FirstSessionId","principalColumn":"Id","ordinal":1}]},{"schema":"dbo","table":"EnrollmentApplications","name":"FK_EnrollmentApplications_Courses_CourseId","principalSchema":"dbo","principalTable":"Courses","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CourseId","principalColumn":"CourseId","ordinal":1}]},{"schema":"dbo","table":"EnrollmentApplications","name":"FK_EnrollmentApplications_Payments_PaymentId","principalSchema":"dbo","principalTable":"Payments","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"PaymentId","principalColumn":"PaymentId","ordinal":1}]},{"schema":"dbo","table":"EnrollmentApplications","name":"FK_EnrollmentApplications_Students_StudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"EnrollmentApplications","name":"FK_EnrollmentApplications_Users_ReviewedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ReviewedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"LeaveRequests","name":"FK_LeaveRequests_ClassSessionParticipants_SessionParticipantId","principalSchema":"dbo","principalTable":"ClassSessionParticipants","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"SessionParticipantId","principalColumn":"SessionParticipantId","ordinal":1}]},{"schema":"dbo","table":"LeaveRequests","name":"FK_LeaveRequests_Users_CancelledByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CancelledByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"LeaveRequests","name":"FK_LeaveRequests_Users_CreatedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CreatedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"LessonPasses","name":"FK_LessonPasses_LessonPlans_LessonPlanId","principalSchema":"dbo","principalTable":"LessonPlans","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"LessonPlanId","principalColumn":"LessonPlanId","ordinal":1}]},{"schema":"dbo","table":"LessonPasses","name":"FK_LessonPasses_Payments_PaymentId","principalSchema":"dbo","principalTable":"Payments","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"PaymentId","principalColumn":"PaymentId","ordinal":1}]},{"schema":"dbo","table":"LessonPasses","name":"FK_LessonPasses_Students_StudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"LessonPasses","name":"FK_LessonPasses_TrialGuests_TrialGuestId","principalSchema":"dbo","principalTable":"TrialGuests","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TrialGuestId","principalColumn":"TrialGuestId","ordinal":1}]},{"schema":"dbo","table":"LessonPasses","name":"FK_LessonPasses_Users_CreatedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CreatedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"MakeupEntitlements","name":"FK_MakeupEntitlements_LeaveRequests_SourceLeaveRequestId","principalSchema":"dbo","principalTable":"LeaveRequests","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"SourceLeaveRequestId","principalColumn":"LeaveRequestId","ordinal":1}]},{"schema":"dbo","table":"MakeupEntitlements","name":"FK_MakeupEntitlements_Students_StudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"MakeupEntitlements","name":"FK_MakeupEntitlements_Terms_SourceTermId","principalSchema":"dbo","principalTable":"Terms","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"SourceTermId","principalColumn":"TermId","ordinal":1}]},{"schema":"dbo","table":"MakeupRequests","name":"FK_MakeupRequests_ClassSessions_TargetClassSessionId","principalSchema":"dbo","principalTable":"ClassSessions","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TargetClassSessionId","principalColumn":"Id","ordinal":1}]},{"schema":"dbo","table":"MakeupRequests","name":"FK_MakeupRequests_MakeupEntitlements_MakeupEntitlementId","principalSchema":"dbo","principalTable":"MakeupEntitlements","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"MakeupEntitlementId","principalColumn":"MakeupEntitlementId","ordinal":1}]},{"schema":"dbo","table":"MakeupRequests","name":"FK_MakeupRequests_Users_CancelledByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CancelledByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"MakeupRequests","name":"FK_MakeupRequests_Users_RequestedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"RequestedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"MakeupRequests","name":"FK_MakeupRequests_Users_ReviewedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ReviewedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"Payments","name":"FK_Payments_Students_StudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"Payments","name":"FK_Payments_TrialGuests_TrialGuestId","principalSchema":"dbo","principalTable":"TrialGuests","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TrialGuestId","principalColumn":"TrialGuestId","ordinal":1}]},{"schema":"dbo","table":"Payments","name":"FK_Payments_Users_ConfirmedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ConfirmedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"PortalTrialRequests","name":"FK_PortalTrialRequests_ClassSessions_ClassSessionId","principalSchema":"dbo","principalTable":"ClassSessions","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ClassSessionId","principalColumn":"Id","ordinal":1}]},{"schema":"dbo","table":"PortalTrialRequests","name":"FK_PortalTrialRequests_LessonPlans_LessonPlanId","principalSchema":"dbo","principalTable":"LessonPlans","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"LessonPlanId","principalColumn":"LessonPlanId","ordinal":1}]},{"schema":"dbo","table":"PortalTrialRequests","name":"FK_PortalTrialRequests_TrialGuests_TrialGuestId","principalSchema":"dbo","principalTable":"TrialGuests","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TrialGuestId","principalColumn":"TrialGuestId","ordinal":1}]},{"schema":"dbo","table":"PortalTrialRequests","name":"FK_PortalTrialRequests_Users_UserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"UserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"Students","name":"FK_Students_Users","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"UserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_ClassSessions_CreatedSessionId","principalSchema":"dbo","principalTable":"ClassSessions","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CreatedSessionId","principalColumn":"Id","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_ClassSessions_TargetSessionId","principalSchema":"dbo","principalTable":"ClassSessions","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TargetSessionId","principalColumn":"Id","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_Courses_CourseId","principalSchema":"dbo","principalTable":"Courses","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CourseId","principalColumn":"CourseId","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_StudioLocations_StudioLocationId","principalSchema":"dbo","principalTable":"StudioLocations","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"StudioLocationId","principalColumn":"StudioLocationId","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_Teachers_TeacherId","principalSchema":"dbo","principalTable":"Teachers","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"TeacherId","principalColumn":"TeacherId","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_Users_ReviewedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ReviewedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"TeacherRequests","name":"FK_TeacherRequests_Users_SubmittedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"SubmittedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"Teachers","name":"FK_Teachers_Users","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"UserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"TrialGuests","name":"FK_TrialGuests_Students_ConvertedStudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ConvertedStudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"TrialGuests","name":"FK_TrialGuests_Students_ReferrerStudentId","principalSchema":"dbo","principalTable":"Students","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"ReferrerStudentId","principalColumn":"StudentId","ordinal":1}]},{"schema":"dbo","table":"TrialGuests","name":"FK_TrialGuests_Users_CreatedByUserId","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"CreatedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"UserRoles","name":"FK_UserRoles_AssignedBy","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"AssignedByUserId","principalColumn":"UserId","ordinal":1}]},{"schema":"dbo","table":"UserRoles","name":"FK_UserRoles_Roles","principalSchema":"dbo","principalTable":"Roles","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"RoleId","principalColumn":"RoleId","ordinal":1}]},{"schema":"dbo","table":"UserRoles","name":"FK_UserRoles_Users","principalSchema":"dbo","principalTable":"Users","onDelete":"NO_ACTION","onUpdate":"NO_ACTION","disabled":false,"notTrusted":false,"columns":[{"column":"UserId","principalColumn":"UserId","ordinal":1}]}],"checks":[{"schema":"dbo","table":"AttendanceRecords","name":"CK_AttendanceRecords_Status","sql":"([Status]=N''Absent'' OR [Status]=N''Late'' OR [Status]=N''Present'' OR [Status]=N''Scheduled'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"ClassSessions","name":"CK_ClassSessions_Capacity","sql":"([Capacity]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"ClassSessions","name":"CK_ClassSessions_CapacityOverride","sql":"([CapacityOverride] IS NULL OR [CapacityOverride]>=[Capacity])","disabled":false,"notTrusted":false},{"schema":"dbo","table":"ClassSessions","name":"CK_ClassSessions_TimeRange","sql":"([EndsAt]>[StartsAt])","disabled":false,"notTrusted":false},{"schema":"dbo","table":"Courses","name":"CK_Courses_DefaultCapacity","sql":"([DefaultCapacity]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"Courses","name":"CK_Courses_TermPrice","sql":"([TermPrice]>=(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"EnrollmentApplications","name":"CK_EnrollmentApplications_QuotedAmount","sql":"([QuotedAmount]>=(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"EnrollmentApplications","name":"CK_EnrollmentApplications_QuotedSessionCount","sql":"([QuotedSessionCount]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"EnrollmentApplications","name":"CK_EnrollmentApplications_Status","sql":"([Status]=N''Cancelled'' OR [Status]=N''Rejected'' OR [Status]=N''Approved'' OR [Status]=N''Pending'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LeaveRequests","name":"CK_LeaveRequests_Status","sql":"([Status]=N''Cancelled'' OR [Status]=N''Active'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPasses","name":"CK_LessonPasses_Amount","sql":"([AmountAtPurchase]>=(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPasses","name":"CK_LessonPasses_DateRange","sql":"([ValidUntilDate] IS NULL OR [ValidUntilDate]>=[ValidFromDate])","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPasses","name":"CK_LessonPasses_Owner","sql":"([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL OR [StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPasses","name":"CK_LessonPasses_Quota","sql":"([SessionQuotaAtPurchase]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPasses","name":"CK_LessonPasses_Status","sql":"([Status]=N''Cancelled'' OR [Status]=N''Active'' OR [Status]=N''Pending'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPlans","name":"CK_LessonPlans_Audience","sql":"([PlanType]=N''Trial'' AND [AudienceType]=N''TrialGuest'' OR [PlanType]=N''Flexible'' AND [AudienceType]=N''Student'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPlans","name":"CK_LessonPlans_Price","sql":"([Price]>=(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPlans","name":"CK_LessonPlans_Quota","sql":"([SessionQuota]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"LessonPlans","name":"CK_LessonPlans_Validity","sql":"([ValidityUnit]=N''Unlimited'' AND [ValidityValue] IS NULL OR ([ValidityUnit]=N''Months'' OR [ValidityUnit]=N''Days'') AND [ValidityValue] IS NOT NULL AND [ValidityValue]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"MakeupEntitlements","name":"CK_MakeupEntitlements_Status","sql":"([Status]=N''Revoked'' OR [Status]=N''Consumed'' OR [Status]=N''Reserved'' OR [Status]=N''Available'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"MakeupRequests","name":"CK_MakeupRequests_Status","sql":"([Status]=N''Cancelled'' OR [Status]=N''Rejected'' OR [Status]=N''Approved'' OR [Status]=N''Pending'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"Payments","name":"CK_Payments_Amount","sql":"([Amount]>=(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"Payments","name":"CK_Payments_Payer","sql":"([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL OR [StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)","disabled":false,"notTrusted":false},{"schema":"dbo","table":"StudioLocations","name":"CK_StudioLocations_Latitude","sql":"([Latitude] IS NULL OR [Latitude]>=(-90) AND [Latitude]<=(90))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"StudioLocations","name":"CK_StudioLocations_Longitude","sql":"([Longitude] IS NULL OR [Longitude]>=(-180) AND [Longitude]<=(180))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"StudioLocations","name":"CK_StudioLocations_Radius","sql":"([CheckInRadiusMeters]>(0))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"TeacherRequests","name":"CK_TeacherRequests_Capacity","sql":"([RequestedCapacity]>=(1) AND [RequestedCapacity]<=(32767))","disabled":false,"notTrusted":false},{"schema":"dbo","table":"TeacherRequests","name":"CK_TeacherRequests_Kind","sql":"([Kind]=N''ExtraSession'' AND [TargetSessionId] IS NULL OR [Kind]=N''Capacity'' AND [TargetSessionId] IS NOT NULL AND [OriginalCapacity] IS NOT NULL AND [TargetVersion] IS NOT NULL)","disabled":false,"notTrusted":false},{"schema":"dbo","table":"TeacherRequests","name":"CK_TeacherRequests_Status","sql":"([Status]=N''Rejected'' OR [Status]=N''Approved'' OR [Status]=N''Pending'')","disabled":false,"notTrusted":false},{"schema":"dbo","table":"TeacherRequests","name":"CK_TeacherRequests_Time","sql":"([EndsAt]>[StartsAt])","disabled":false,"notTrusted":false},{"schema":"dbo","table":"Terms","name":"CK_Terms_DateRange","sql":"([StartDate]<=[EndDate])","disabled":false,"notTrusted":false}]}'),('T',N'{
  "databaseName": "EnjoySpaceDb",
  "capturedAtUtc": "2026-11-02T02:04:56.6839099",
  "migrations": [
    {
      "MigrationId": "20260920144316_InitialCreate",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260921153358_AddCourseRowVersion",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260921163921_AddTeachersAndLocations",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260921165340_AddClassSessions",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260922123750_AddClassSessionChecks",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260923053339_AddStudents",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260923062750_AddEnrollmentApplications",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260923075505_AddEnrollmentApplicationStatusCheck",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260925073616_AddUsers",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260925084606_AddRoles",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260926081020_AddUserRoles",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928053810_AddTerms",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928054501_AddClassCategories",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928064739_AddStudioLocations",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928075158_LinkSessionsToStudioLocations",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928081846_SwitchSessionsToStudioLocations",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928091511_RemoveLegacyLocations",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928092342_AddTeacherUserLink",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928095018_AddTeacherProfileFields",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928102121_RequireTeacherUser",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928114431_RemoveLegacyTeacherName",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928115922_RemoveLegacyTeacherRowVersion",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928124730_RenameTeacherPrimaryKey",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928125635_ChangeTeacherIdToBigInt",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928134630_AddStudentUserLink",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928140921_AddStudentProfileFields",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928141934_RequireStudentUser",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928143153_RemoveLegacyStudentContactFields",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928143735_RemoveLegacyStudentRowVersion",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928144059_RenameStudentPrimaryKey",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928144645_ChangeStudentIdToBigInt",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260928145601_AddCourseTermLink",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929015158_RequireCourseTerm",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929022545_AddCourseClassCategoryLink",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929040912_RequireCourseClassCategory",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929055002_AddCourseDefaultTeacherLink",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929064929_RequireCourseDefaultTeacher",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929065658_AddCourseDefaultLocationLink",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929073952_RequireCourseDefaultLocation",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929080426_CompleteCourseBasicFields",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260929144821_AlignCourseIdentityAndTermDates",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20260930034905_AddEnrollmentPaymentWorkflow",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261006144721_ScheduleConfirmationAndEnrollmentConsent",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261008072606_AddSessionParticipantsAndAttendance",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261011123639_AddLeaveAndMakeupWorkflow",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261011131928_AddTrialPlansAndPurchases",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261011174223_AddPortalTrialRequests",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261011183856_AddTeacherRequests",
      "ProductVersion": "10.0.12"
    },
    {
      "MigrationId": "20261016081127_AddPortalTrialReferrals",
      "ProductVersion": "10.0.13"
    },
    {
      "MigrationId": "20261016081308_SupportMonthlyLessonPlans",
      "ProductVersion": "10.0.13"
    },
    {
      "MigrationId": "20261022040727_AddStudentFlexibleBookings",
      "ProductVersion": "10.0.13"
    },
    {
      "MigrationId": "20261023165439_AddQrCheckInTimestamp",
      "ProductVersion": "10.0.13"
    },
    {
      "MigrationId": "20261025064123_AddSessionOperations",
      "ProductVersion": "10.0.13"
    },
    {
      "MigrationId": "20261026093213_AddTrialSessionEnrollment",
      "ProductVersion": "10.0.14"
    }
  ],
  "tables": [
    {
      "schema": "dbo",
      "table": "__EFMigrationsHistory"
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords"
    },
    {
      "schema": "dbo",
      "table": "ClassCategories"
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants"
    },
    {
      "schema": "dbo",
      "table": "ClassSessions"
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments"
    },
    {
      "schema": "dbo",
      "table": "Courses"
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications"
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests"
    },
    {
      "schema": "dbo",
      "table": "LessonPasses"
    },
    {
      "schema": "dbo",
      "table": "LessonPlans"
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements"
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests"
    },
    {
      "schema": "dbo",
      "table": "Payments"
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests"
    },
    {
      "schema": "dbo",
      "table": "Roles"
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges"
    },
    {
      "schema": "dbo",
      "table": "Students"
    },
    {
      "schema": "dbo",
      "table": "StudioLocations"
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests"
    },
    {
      "schema": "dbo",
      "table": "Teachers"
    },
    {
      "schema": "dbo",
      "table": "Terms"
    },
    {
      "schema": "dbo",
      "table": "TrialGuests"
    },
    {
      "schema": "dbo",
      "table": "UserRoles"
    },
    {
      "schema": "dbo",
      "table": "Users"
    }
  ],
  "columns": [
    {
      "schema": "dbo",
      "table": "__EFMigrationsHistory",
      "column": "MigrationId",
      "type": "nvarchar",
      "maxLengthBytes": 300,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "__EFMigrationsHistory",
      "column": "ProductVersion",
      "type": "nvarchar",
      "maxLengthBytes": 64,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "AttendanceRecordId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "SessionParticipantId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "CheckInMethod",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "CheckedInByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "ManualReason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "UpdatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "column": "CheckedInAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassCategories",
      "column": "ClassCategoryId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassCategories",
      "column": "Code",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassCategories",
      "column": "Name",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassCategories",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_ClassCategories_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "SessionParticipantId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "ClassSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "CourseEnrollmentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "ParticipationType",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "AddedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "AddedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__ClassSess__Added__756D6ECB",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "MakeupRequestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "BookingKey",
      "type": "uniqueidentifier",
      "maxLengthBytes": 16,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "CancellationReason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "CancelledAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 8,
      "precision": 27,
      "scale": 7,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "LessonPassId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "column": "TrialRequestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "Id",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "CourseId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "TeacherId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "StartsAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "EndsAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "Capacity",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "CapacityOverride",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "StudioLocationId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "IsExtraSession",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__ClassSess__IsExt__32767D0B",
      "defaultSql": "(CONVERT([bit],(0)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "CancellationReason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "CancelledAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "CancelledByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "column": "IsCancelled",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__ClassSess__IsCan__43A1090D",
      "defaultSql": "(CONVERT([bit],(0)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "CourseEnrollmentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "CourseId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "EnrollmentApplicationId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "EnrolledAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__CourseEnr__Enrol__4E53A1AA",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "CancelledAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "CreatedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "CourseId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "Name",
      "type": "nvarchar",
      "maxLengthBytes": 300,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "TermId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "ClassCategoryId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "DefaultTeacherId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "DefaultLocationId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "AllowsFlexibleBooking",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Courses_AllowsFlexibleBooking",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "AllowsMakeup",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Courses_AllowsMakeup",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Courses_CreatedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "DefaultCapacity",
      "type": "smallint",
      "maxLengthBytes": 2,
      "precision": 5,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Courses_DefaultCapacity",
      "defaultSql": "(CONVERT([smallint],(10)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Courses_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "IsPubliclyVisible",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Courses_IsPubliclyVisible",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "column": "TermPrice",
      "type": "decimal",
      "maxLengthBytes": 9,
      "precision": 10,
      "scale": 2,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "EnrollmentApplicationId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "CourseId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "AppliedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "ReviewedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "ReviewNote",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "ApplicationType",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": "DF__Enrollmen__Appli__46B27FE2",
      "defaultSql": "(N\u0027FullTerm\u0027)",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "FirstSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "PaymentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "QuotedAmount",
      "type": "decimal",
      "maxLengthBytes": 9,
      "precision": 10,
      "scale": 2,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__Enrollmen__Quote__47A6A41B",
      "defaultSql": "((0.0))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "QuotedSessionCount",
      "type": "smallint",
      "maxLengthBytes": 2,
      "precision": 5,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__Enrollmen__Quote__489AC854",
      "defaultSql": "(CONVERT([smallint],(0)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "ReviewedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "ConfirmedPolicy",
      "type": "nvarchar",
      "maxLengthBytes": 400,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "PolicyConfirmedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "column": "PolicyConfirmedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "LeaveRequestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "SessionParticipantId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "RequestSource",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "Reason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "CreatedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "RequestedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "CancelledByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "CancelledAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "LessonPassId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "PurchaseKey",
      "type": "uniqueidentifier",
      "maxLengthBytes": 16,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "LessonPlanId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "TrialGuestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "PaymentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "PlanNameAtPurchase",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "PlanTypeAtPurchase",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "SessionQuotaAtPurchase",
      "type": "smallint",
      "maxLengthBytes": 2,
      "precision": 5,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "AmountAtPurchase",
      "type": "decimal",
      "maxLengthBytes": 9,
      "precision": 10,
      "scale": 2,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "ValidFromDate",
      "type": "date",
      "maxLengthBytes": 3,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "ValidUntilDate",
      "type": "date",
      "maxLengthBytes": 3,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "PurchasedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "CreatedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "Note",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "LessonPlanId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "PlanCode",
      "type": "nvarchar",
      "maxLengthBytes": 100,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "PlanName",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "PlanType",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "AudienceType",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "SessionQuota",
      "type": "smallint",
      "maxLengthBytes": 2,
      "precision": 5,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "Price",
      "type": "decimal",
      "maxLengthBytes": 9,
      "precision": 10,
      "scale": 2,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "ValidityUnit",
      "type": "nvarchar",
      "maxLengthBytes": 40,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "ValidityValue",
      "type": "smallint",
      "maxLengthBytes": 2,
      "precision": 5,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "column": "RequiresReferrer",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__LessonPla__Requi__3FD07829",
      "defaultSql": "(CONVERT([bit],(0)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "MakeupEntitlementId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "SourceTermId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "SourceLeaveRequestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "SourceType",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "ConsumedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "RevokedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "column": "SourceCancellationParticipantId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "MakeupRequestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "MakeupEntitlementId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "TargetClassSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "RequestedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "RequestedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "ReviewedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "ReviewedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "ReviewNote",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "CancelledByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "CancelledAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "CancellationReason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "PaymentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "TrialGuestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "PaymentPurpose",
      "type": "nvarchar",
      "maxLengthBytes": 100,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "Amount",
      "type": "decimal",
      "maxLengthBytes": 9,
      "precision": 10,
      "scale": 2,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "PaymentMethod",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "PaidAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "ConfirmedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "ConfirmedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "Note",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__Payments__Create__5AB9788F",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "Id",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "SubmissionKey",
      "type": "uniqueidentifier",
      "maxLengthBytes": 16,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "UserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "TrialGuestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "ClassSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "LessonPlanId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "PlanName",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "QuotedPrice",
      "type": "decimal",
      "maxLengthBytes": 9,
      "precision": 10,
      "scale": 2,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "Note",
      "type": "nvarchar",
      "maxLengthBytes": 600,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 8,
      "precision": 27,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "ReferrerName",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "LessonPassId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "ReviewNote",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "ReviewedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 8,
      "precision": 27,
      "scale": 7,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "ReviewedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Roles",
      "column": "RoleId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Roles",
      "column": "RoleCode",
      "type": "nvarchar",
      "maxLengthBytes": 100,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Roles",
      "column": "RoleName",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Roles",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Roles_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "Id",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "ClassSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "PreviousTeacherId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "TeacherId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "ChangedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "ChangedAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "column": "Reason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "StudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "UserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Students_CreatedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Students_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "JoinedDate",
      "type": "date",
      "maxLengthBytes": 3,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "Note",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Students",
      "column": "StudentNo",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "StudioLocationId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "Name",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "Address",
      "type": "nvarchar",
      "maxLengthBytes": 600,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "Latitude",
      "type": "decimal",
      "maxLengthBytes": 5,
      "precision": 9,
      "scale": 6,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "Longitude",
      "type": "decimal",
      "maxLengthBytes": 5,
      "precision": 9,
      "scale": 6,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "CheckInRadiusMeters",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_StudioLocations_Radius",
      "defaultSql": "((500))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_StudioLocations_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_StudioLocations_CreatedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "Id",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "SubmissionKey",
      "type": "uniqueidentifier",
      "maxLengthBytes": 16,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "Kind",
      "type": "nvarchar",
      "maxLengthBytes": 40,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "TeacherId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "CourseId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "TargetSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "CreatedSessionId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "StudioLocationId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "StartsAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "EndsAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "RequestedCapacity",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "OriginalCapacity",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "TargetVersion",
      "type": "varbinary",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "Reason",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 40,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "SubmittedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "SubmittedAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "ReviewedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "ReviewedAt",
      "type": "datetimeoffset",
      "maxLengthBytes": 10,
      "precision": 34,
      "scale": 7,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "ReviewNote",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "column": "TeacherId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "column": "UserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Teachers_CreatedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Teachers_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "column": "Note",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "column": "TeacherNo",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "TermId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "Name",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "StartDate",
      "type": "date",
      "maxLengthBytes": 3,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "EndDate",
      "type": "date",
      "maxLengthBytes": 3,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "RegistrationStartAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "RegistrationEndAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": "DF_Terms_Status",
      "defaultSql": "(N\u0027Draft\u0027)",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Terms_CreatedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "ScheduleConfirmedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "column": "ScheduleConfirmedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "TrialGuestId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "Name",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "Phone",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "Email",
      "type": "nvarchar",
      "maxLengthBytes": 400,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "LineId",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "SourceType",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "ReferrerStudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "ConvertedStudentId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "Status",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "Note",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "CreatedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF__TrialGues__Creat__55009F39",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "UpdatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "column": "UserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "column": "RoleId",
      "type": "int",
      "maxLengthBytes": 4,
      "precision": 10,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "column": "AssignedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_UserRoles_AssignedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "column": "AssignedByUserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "UserId",
      "type": "bigint",
      "maxLengthBytes": 8,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": true,
      "identitySeed": "1",
      "identityIncrement": "1",
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "Account",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "PasswordHash",
      "type": "nvarchar",
      "maxLengthBytes": 1000,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "DisplayName",
      "type": "nvarchar",
      "maxLengthBytes": 200,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "Email",
      "type": "nvarchar",
      "maxLengthBytes": 400,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "Phone",
      "type": "nvarchar",
      "maxLengthBytes": 60,
      "precision": 0,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": "Chinese_Taiwan_Stroke_CI_AS",
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "IsActive",
      "type": "bit",
      "maxLengthBytes": 1,
      "precision": 1,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Users_IsActive",
      "defaultSql": "(CONVERT([bit],(1)))",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "CreatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": "DF_Users_CreatedAtUtc",
      "defaultSql": "(sysutcdatetime())",
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "UpdatedAtUtc",
      "type": "datetime2",
      "maxLengthBytes": 6,
      "precision": 19,
      "scale": 0,
      "nullable": true,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    },
    {
      "schema": "dbo",
      "table": "Users",
      "column": "RowVersion",
      "type": "timestamp",
      "maxLengthBytes": 8,
      "precision": 0,
      "scale": 0,
      "nullable": false,
      "identity": false,
      "identitySeed": null,
      "identityIncrement": null,
      "collation": null,
      "defaultName": null,
      "defaultSql": null,
      "computedSql": null,
      "computedPersisted": null
    }
  ],
  "indexes": [
    {
      "schema": "dbo",
      "table": "__EFMigrationsHistory",
      "name": "PK___EFMigrationsHistory",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "MigrationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "IX_AttendanceRecords_CheckedInByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CheckedInByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "IX_AttendanceRecords_SessionParticipantId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "SessionParticipantId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "IX_AttendanceRecords_Status",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Status",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "PK_AttendanceRecords",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "AttendanceRecordId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassCategories",
      "name": "PK_ClassCategories",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ClassCategoryId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassCategories",
      "name": "UQ_ClassCategories_Code",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Code",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_AddedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "AddedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_BookingKey",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([BookingKey] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "BookingKey",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_ClassSessionId_CourseEnrollmentId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([ParticipationType]=N\u0027Regular\u0027)",
      "disabled": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        },
        {
          "column": "CourseEnrollmentId",
          "ordinal": 2,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_CourseEnrollmentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseEnrollmentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_LessonPassId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LessonPassId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_MakeupRequestId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([MakeupRequestId] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "MakeupRequestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_StudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "IX_ClassSessionParticipants_TrialRequestId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([TrialRequestId] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "TrialRequestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "PK_ClassSessionParticipants",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "SessionParticipantId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "UX_ClassSessionParticipants_ActiveStudent",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([Status]=N\u0027Active\u0027)",
      "disabled": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        },
        {
          "column": "StudentId",
          "ordinal": 2,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "IX_ClassSessions_CancelledByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CancelledByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "IX_ClassSessions_CourseId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "IX_ClassSessions_StudioLocationId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudioLocationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "IX_ClassSessions_TeacherId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TeacherId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "PK_ClassSessions",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Id",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "IX_CourseEnrollments_CreatedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "IX_CourseEnrollments_StudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "PK_CourseEnrollments",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseEnrollmentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "UQ_CourseEnrollments_Application",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "EnrollmentApplicationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "UQ_CourseEnrollments_Course_Student",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseId",
          "ordinal": 1,
          "descending": false,
          "included": false
        },
        {
          "column": "StudentId",
          "ordinal": 2,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "IX_Courses_ClassCategoryId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ClassCategoryId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "IX_Courses_DefaultLocationId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "DefaultLocationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "IX_Courses_DefaultTeacherId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "DefaultTeacherId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "IX_Courses_TermId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TermId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "PK_Courses",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "IX_EnrollmentApplications_CourseId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "IX_EnrollmentApplications_FirstSessionId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "FirstSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "IX_EnrollmentApplications_PaymentId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([PaymentId] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "PaymentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "IX_EnrollmentApplications_ReviewedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "IX_EnrollmentApplications_Status",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Status",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "IX_EnrollmentApplications_StudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "PK_EnrollmentApplications",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "EnrollmentApplicationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "IX_LeaveRequests_CancelledByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CancelledByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "IX_LeaveRequests_CreatedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "IX_LeaveRequests_SessionParticipantId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([Status]=N\u0027Active\u0027)",
      "disabled": false,
      "columns": [
        {
          "column": "SessionParticipantId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "PK_LeaveRequests",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LeaveRequestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "IX_LessonPasses_CreatedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "IX_LessonPasses_LessonPlanId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LessonPlanId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "IX_LessonPasses_PaymentId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "PaymentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "IX_LessonPasses_PurchaseKey",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "PurchaseKey",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "IX_LessonPasses_StudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "IX_LessonPasses_TrialGuestId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "PK_LessonPasses",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LessonPassId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "name": "IX_LessonPlans_PlanCode",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "PlanCode",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "name": "PK_LessonPlans",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LessonPlanId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "IX_MakeupEntitlements_SourceCancellationParticipantId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([SourceCancellationParticipantId] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "SourceCancellationParticipantId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "IX_MakeupEntitlements_SourceLeaveRequestId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([SourceLeaveRequestId] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "SourceLeaveRequestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "IX_MakeupEntitlements_SourceTermId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "SourceTermId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "IX_MakeupEntitlements_StudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "PK_MakeupEntitlements",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "MakeupEntitlementId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "IX_MakeupRequests_CancelledByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CancelledByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "IX_MakeupRequests_MakeupEntitlementId_Status",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "MakeupEntitlementId",
          "ordinal": 1,
          "descending": false,
          "included": false
        },
        {
          "column": "Status",
          "ordinal": 2,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "IX_MakeupRequests_RequestedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "RequestedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "IX_MakeupRequests_ReviewedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "IX_MakeupRequests_TargetClassSessionId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TargetClassSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "PK_MakeupRequests",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "MakeupRequestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "IX_Payments_ConfirmedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ConfirmedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "IX_Payments_Status",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Status",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "IX_Payments_StudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "IX_Payments_TrialGuestId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "PK_Payments",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "PaymentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_ClassSessionId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_LessonPassId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LessonPassId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_LessonPlanId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "LessonPlanId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_ReviewedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_SubmissionKey",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "SubmissionKey",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_TrialGuestId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "IX_PortalTrialRequests_UserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "UserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "PK_PortalTrialRequests",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Id",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Roles",
      "name": "PK_Roles",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "RoleId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Roles",
      "name": "UQ_Roles_RoleCode",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "RoleCode",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "IX_SessionTeacherChanges_ChangedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ChangedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "IX_SessionTeacherChanges_ClassSessionId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "IX_SessionTeacherChanges_PreviousTeacherId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "PreviousTeacherId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "IX_SessionTeacherChanges_TeacherId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TeacherId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "PK_SessionTeacherChanges",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Id",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Students",
      "name": "PK_Students",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Students",
      "name": "UQ_Students_UserId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "UserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Students",
      "name": "UX_Students_StudentNo",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([StudentNo] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "StudentNo",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "name": "PK_StudioLocations",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudioLocationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_CourseId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CourseId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_CreatedSessionId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([CreatedSessionId] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "CreatedSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_ReviewedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_StudioLocationId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "StudioLocationId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_SubmissionKey",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "SubmissionKey",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_SubmittedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "SubmittedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_TargetSessionId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([Kind]=N\u0027Capacity\u0027 AND [Status]=N\u0027Pending\u0027)",
      "disabled": false,
      "columns": [
        {
          "column": "TargetSessionId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "IX_TeacherRequests_TeacherId_Status",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TeacherId",
          "ordinal": 1,
          "descending": false,
          "included": false
        },
        {
          "column": "Status",
          "ordinal": 2,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "PK_TeacherRequests",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Id",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "name": "PK_Teachers",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TeacherId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "name": "UQ_Teachers_UserId",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "UserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "name": "UX_Teachers_TeacherNo",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": "([TeacherNo] IS NOT NULL)",
      "disabled": false,
      "columns": [
        {
          "column": "TeacherNo",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "name": "PK_Terms",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TermId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "IX_TrialGuests_ConvertedStudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ConvertedStudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "IX_TrialGuests_CreatedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "IX_TrialGuests_Email",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Email",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "IX_TrialGuests_Phone",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Phone",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "IX_TrialGuests_ReferrerStudentId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "ReferrerStudentId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "PK_TrialGuests",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "name": "IX_UserRoles_AssignedByUserId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "AssignedByUserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "name": "IX_UserRoles_RoleId",
      "type": "NONCLUSTERED",
      "isUnique": false,
      "isPrimaryKey": false,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "RoleId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "name": "PK_UserRoles",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "UserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        },
        {
          "column": "RoleId",
          "ordinal": 2,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Users",
      "name": "PK_Users",
      "type": "CLUSTERED",
      "isUnique": true,
      "isPrimaryKey": true,
      "isUniqueConstraint": false,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "UserId",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Users",
      "name": "UQ_Users_Account",
      "type": "NONCLUSTERED",
      "isUnique": true,
      "isPrimaryKey": false,
      "isUniqueConstraint": true,
      "filterSql": null,
      "disabled": false,
      "columns": [
        {
          "column": "Account",
          "ordinal": 1,
          "descending": false,
          "included": false
        }
      ]
    }
  ],
  "foreignKeys": [
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "FK_AttendanceRecords_ClassSessionParticipants_SessionParticipantId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessionParticipants",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "SessionParticipantId",
          "principalColumn": "SessionParticipantId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "FK_AttendanceRecords_Users_CheckedInByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CheckedInByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_ClassSessions_ClassSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_CourseEnrollments_CourseEnrollmentId",
      "principalSchema": "dbo",
      "principalTable": "CourseEnrollments",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CourseEnrollmentId",
          "principalColumn": "CourseEnrollmentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_LessonPasses_LessonPassId",
      "principalSchema": "dbo",
      "principalTable": "LessonPasses",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "LessonPassId",
          "principalColumn": "LessonPassId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_MakeupRequests_MakeupRequestId",
      "principalSchema": "dbo",
      "principalTable": "MakeupRequests",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "MakeupRequestId",
          "principalColumn": "MakeupRequestId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_PortalTrialRequests_TrialRequestId",
      "principalSchema": "dbo",
      "principalTable": "PortalTrialRequests",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TrialRequestId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_Students_StudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "FK_ClassSessionParticipants_Users_AddedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "AddedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "FK_ClassSessions_Courses_CourseId",
      "principalSchema": "dbo",
      "principalTable": "Courses",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CourseId",
          "principalColumn": "CourseId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "FK_ClassSessions_Locations",
      "principalSchema": "dbo",
      "principalTable": "StudioLocations",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudioLocationId",
          "principalColumn": "StudioLocationId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "FK_ClassSessions_Teachers_TeacherId",
      "principalSchema": "dbo",
      "principalTable": "Teachers",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TeacherId",
          "principalColumn": "TeacherId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "FK_ClassSessions_Users_CancelledByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CancelledByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "FK_CourseEnrollments_Courses_CourseId",
      "principalSchema": "dbo",
      "principalTable": "Courses",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CourseId",
          "principalColumn": "CourseId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "FK_CourseEnrollments_EnrollmentApplications_EnrollmentApplicationId",
      "principalSchema": "dbo",
      "principalTable": "EnrollmentApplications",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "EnrollmentApplicationId",
          "principalColumn": "EnrollmentApplicationId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "FK_CourseEnrollments_Students_StudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "CourseEnrollments",
      "name": "FK_CourseEnrollments_Users_CreatedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "FK_Courses_ClassCategories",
      "principalSchema": "dbo",
      "principalTable": "ClassCategories",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ClassCategoryId",
          "principalColumn": "ClassCategoryId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "FK_Courses_DefaultLocation",
      "principalSchema": "dbo",
      "principalTable": "StudioLocations",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "DefaultLocationId",
          "principalColumn": "StudioLocationId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "FK_Courses_DefaultTeacher",
      "principalSchema": "dbo",
      "principalTable": "Teachers",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "DefaultTeacherId",
          "principalColumn": "TeacherId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "FK_Courses_Terms",
      "principalSchema": "dbo",
      "principalTable": "Terms",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TermId",
          "principalColumn": "TermId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "FK_EnrollmentApplications_ClassSessions_FirstSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "FirstSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "FK_EnrollmentApplications_Courses_CourseId",
      "principalSchema": "dbo",
      "principalTable": "Courses",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CourseId",
          "principalColumn": "CourseId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "FK_EnrollmentApplications_Payments_PaymentId",
      "principalSchema": "dbo",
      "principalTable": "Payments",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "PaymentId",
          "principalColumn": "PaymentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "FK_EnrollmentApplications_Students_StudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "FK_EnrollmentApplications_Users_ReviewedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "FK_LeaveRequests_ClassSessionParticipants_SessionParticipantId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessionParticipants",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "SessionParticipantId",
          "principalColumn": "SessionParticipantId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "FK_LeaveRequests_Users_CancelledByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CancelledByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "FK_LeaveRequests_Users_CreatedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "FK_LessonPasses_LessonPlans_LessonPlanId",
      "principalSchema": "dbo",
      "principalTable": "LessonPlans",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "LessonPlanId",
          "principalColumn": "LessonPlanId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "FK_LessonPasses_Payments_PaymentId",
      "principalSchema": "dbo",
      "principalTable": "Payments",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "PaymentId",
          "principalColumn": "PaymentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "FK_LessonPasses_Students_StudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "FK_LessonPasses_TrialGuests_TrialGuestId",
      "principalSchema": "dbo",
      "principalTable": "TrialGuests",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "principalColumn": "TrialGuestId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "FK_LessonPasses_Users_CreatedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "FK_MakeupEntitlements_ClassSessionParticipants_SourceCancellationParticipantId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessionParticipants",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "SourceCancellationParticipantId",
          "principalColumn": "SessionParticipantId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "FK_MakeupEntitlements_LeaveRequests_SourceLeaveRequestId",
      "principalSchema": "dbo",
      "principalTable": "LeaveRequests",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "SourceLeaveRequestId",
          "principalColumn": "LeaveRequestId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "FK_MakeupEntitlements_Students_StudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "FK_MakeupEntitlements_Terms_SourceTermId",
      "principalSchema": "dbo",
      "principalTable": "Terms",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "SourceTermId",
          "principalColumn": "TermId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "FK_MakeupRequests_ClassSessions_TargetClassSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TargetClassSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "FK_MakeupRequests_MakeupEntitlements_MakeupEntitlementId",
      "principalSchema": "dbo",
      "principalTable": "MakeupEntitlements",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "MakeupEntitlementId",
          "principalColumn": "MakeupEntitlementId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "FK_MakeupRequests_Users_CancelledByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CancelledByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "FK_MakeupRequests_Users_RequestedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "RequestedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "FK_MakeupRequests_Users_ReviewedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "FK_Payments_Students_StudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "FK_Payments_TrialGuests_TrialGuestId",
      "principalSchema": "dbo",
      "principalTable": "TrialGuests",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "principalColumn": "TrialGuestId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "FK_Payments_Users_ConfirmedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ConfirmedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "FK_PortalTrialRequests_ClassSessions_ClassSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "FK_PortalTrialRequests_LessonPasses_LessonPassId",
      "principalSchema": "dbo",
      "principalTable": "LessonPasses",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "LessonPassId",
          "principalColumn": "LessonPassId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "FK_PortalTrialRequests_LessonPlans_LessonPlanId",
      "principalSchema": "dbo",
      "principalTable": "LessonPlans",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "LessonPlanId",
          "principalColumn": "LessonPlanId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "FK_PortalTrialRequests_TrialGuests_TrialGuestId",
      "principalSchema": "dbo",
      "principalTable": "TrialGuests",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TrialGuestId",
          "principalColumn": "TrialGuestId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "FK_PortalTrialRequests_Users_ReviewedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "PortalTrialRequests",
      "name": "FK_PortalTrialRequests_Users_UserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "UserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "FK_SessionTeacherChanges_ClassSessions_ClassSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ClassSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "FK_SessionTeacherChanges_Teachers_PreviousTeacherId",
      "principalSchema": "dbo",
      "principalTable": "Teachers",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "PreviousTeacherId",
          "principalColumn": "TeacherId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "FK_SessionTeacherChanges_Teachers_TeacherId",
      "principalSchema": "dbo",
      "principalTable": "Teachers",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TeacherId",
          "principalColumn": "TeacherId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "SessionTeacherChanges",
      "name": "FK_SessionTeacherChanges_Users_ChangedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ChangedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Students",
      "name": "FK_Students_Users",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "UserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_ClassSessions_CreatedSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CreatedSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_ClassSessions_TargetSessionId",
      "principalSchema": "dbo",
      "principalTable": "ClassSessions",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TargetSessionId",
          "principalColumn": "Id",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_Courses_CourseId",
      "principalSchema": "dbo",
      "principalTable": "Courses",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CourseId",
          "principalColumn": "CourseId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_StudioLocations_StudioLocationId",
      "principalSchema": "dbo",
      "principalTable": "StudioLocations",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "StudioLocationId",
          "principalColumn": "StudioLocationId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_Teachers_TeacherId",
      "principalSchema": "dbo",
      "principalTable": "Teachers",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "TeacherId",
          "principalColumn": "TeacherId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_Users_ReviewedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ReviewedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "FK_TeacherRequests_Users_SubmittedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "SubmittedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "Teachers",
      "name": "FK_Teachers_Users",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "UserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "FK_TrialGuests_Students_ConvertedStudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ConvertedStudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "FK_TrialGuests_Students_ReferrerStudentId",
      "principalSchema": "dbo",
      "principalTable": "Students",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "ReferrerStudentId",
          "principalColumn": "StudentId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "TrialGuests",
      "name": "FK_TrialGuests_Users_CreatedByUserId",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "CreatedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "name": "FK_UserRoles_AssignedBy",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "AssignedByUserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "name": "FK_UserRoles_Roles",
      "principalSchema": "dbo",
      "principalTable": "Roles",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "RoleId",
          "principalColumn": "RoleId",
          "ordinal": 1
        }
      ]
    },
    {
      "schema": "dbo",
      "table": "UserRoles",
      "name": "FK_UserRoles_Users",
      "principalSchema": "dbo",
      "principalTable": "Users",
      "onDelete": "NO_ACTION",
      "onUpdate": "NO_ACTION",
      "disabled": false,
      "notTrusted": false,
      "columns": [
        {
          "column": "UserId",
          "principalColumn": "UserId",
          "ordinal": 1
        }
      ]
    }
  ],
  "checks": [
    {
      "schema": "dbo",
      "table": "AttendanceRecords",
      "name": "CK_AttendanceRecords_Status",
      "sql": "([Status]=N\u0027Absent\u0027 OR [Status]=N\u0027Late\u0027 OR [Status]=N\u0027Present\u0027 OR [Status]=N\u0027Scheduled\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "ClassSessionParticipants",
      "name": "CK_ClassSessionParticipants_Source",
      "sql": "([ParticipationType]=N\u0027Regular\u0027 AND [CourseEnrollmentId] IS NOT NULL AND [LessonPassId] IS NULL OR [ParticipationType]=N\u0027Makeup\u0027 AND [MakeupRequestId] IS NOT NULL AND [LessonPassId] IS NULL OR [ParticipationType]=N\u0027Trial\u0027 AND [CourseEnrollmentId] IS NULL AND [LessonPassId] IS NOT NULL AND [TrialRequestId] IS NOT NULL OR [ParticipationType]=N\u0027Flexible\u0027 AND [CourseEnrollmentId] IS NULL AND [LessonPassId] IS NOT NULL AND [BookingKey] IS NOT NULL)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "CK_ClassSessions_Capacity",
      "sql": "([Capacity]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "CK_ClassSessions_CapacityOverride",
      "sql": "([CapacityOverride] IS NULL OR [CapacityOverride]\u003E=[Capacity])",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "ClassSessions",
      "name": "CK_ClassSessions_TimeRange",
      "sql": "([EndsAt]\u003E[StartsAt])",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "CK_Courses_DefaultCapacity",
      "sql": "([DefaultCapacity]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "Courses",
      "name": "CK_Courses_TermPrice",
      "sql": "([TermPrice]\u003E=(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "CK_EnrollmentApplications_QuotedAmount",
      "sql": "([QuotedAmount]\u003E=(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "CK_EnrollmentApplications_QuotedSessionCount",
      "sql": "([QuotedSessionCount]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "EnrollmentApplications",
      "name": "CK_EnrollmentApplications_Status",
      "sql": "([Status]=N\u0027Cancelled\u0027 OR [Status]=N\u0027Rejected\u0027 OR [Status]=N\u0027Approved\u0027 OR [Status]=N\u0027Pending\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LeaveRequests",
      "name": "CK_LeaveRequests_Status",
      "sql": "([Status]=N\u0027Cancelled\u0027 OR [Status]=N\u0027Active\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "CK_LessonPasses_Amount",
      "sql": "([AmountAtPurchase]\u003E=(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "CK_LessonPasses_DateRange",
      "sql": "([ValidUntilDate] IS NULL OR [ValidUntilDate]\u003E=[ValidFromDate])",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "CK_LessonPasses_Owner",
      "sql": "([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL OR [StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "CK_LessonPasses_Quota",
      "sql": "([SessionQuotaAtPurchase]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPasses",
      "name": "CK_LessonPasses_Status",
      "sql": "([Status]=N\u0027Cancelled\u0027 OR [Status]=N\u0027Active\u0027 OR [Status]=N\u0027Pending\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "name": "CK_LessonPlans_Audience",
      "sql": "([PlanType]=N\u0027Trial\u0027 AND [AudienceType]=N\u0027TrialGuest\u0027 OR [PlanType]=N\u0027Flexible\u0027 AND [AudienceType]=N\u0027Student\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "name": "CK_LessonPlans_Price",
      "sql": "([Price]\u003E=(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "name": "CK_LessonPlans_Quota",
      "sql": "([SessionQuota]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "LessonPlans",
      "name": "CK_LessonPlans_Validity",
      "sql": "([ValidityUnit]=N\u0027Unlimited\u0027 AND [ValidityValue] IS NULL OR ([ValidityUnit]=N\u0027Months\u0027 OR [ValidityUnit]=N\u0027Days\u0027) AND [ValidityValue] IS NOT NULL AND [ValidityValue]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "CK_MakeupEntitlements_Source",
      "sql": "([SourceType]=N\u0027Leave\u0027 AND [SourceLeaveRequestId] IS NOT NULL AND [SourceCancellationParticipantId] IS NULL OR [SourceType]=N\u0027Cancellation\u0027 AND [SourceLeaveRequestId] IS NULL AND [SourceCancellationParticipantId] IS NOT NULL)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "MakeupEntitlements",
      "name": "CK_MakeupEntitlements_Status",
      "sql": "([Status]=N\u0027Revoked\u0027 OR [Status]=N\u0027Consumed\u0027 OR [Status]=N\u0027Reserved\u0027 OR [Status]=N\u0027Available\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "MakeupRequests",
      "name": "CK_MakeupRequests_Status",
      "sql": "([Status]=N\u0027Cancelled\u0027 OR [Status]=N\u0027Rejected\u0027 OR [Status]=N\u0027Approved\u0027 OR [Status]=N\u0027Pending\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "CK_Payments_Amount",
      "sql": "([Amount]\u003E=(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "Payments",
      "name": "CK_Payments_Payer",
      "sql": "([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL OR [StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "name": "CK_StudioLocations_Latitude",
      "sql": "([Latitude] IS NULL OR [Latitude]\u003E=(-90) AND [Latitude]\u003C=(90))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "name": "CK_StudioLocations_Longitude",
      "sql": "([Longitude] IS NULL OR [Longitude]\u003E=(-180) AND [Longitude]\u003C=(180))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "StudioLocations",
      "name": "CK_StudioLocations_Radius",
      "sql": "([CheckInRadiusMeters]\u003E(0))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "CK_TeacherRequests_Capacity",
      "sql": "([RequestedCapacity]\u003E=(1) AND [RequestedCapacity]\u003C=(32767))",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "CK_TeacherRequests_Kind",
      "sql": "([Kind]=N\u0027ExtraSession\u0027 AND [TargetSessionId] IS NULL OR [Kind]=N\u0027Capacity\u0027 AND [TargetSessionId] IS NOT NULL AND [OriginalCapacity] IS NOT NULL AND [TargetVersion] IS NOT NULL)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "CK_TeacherRequests_Status",
      "sql": "([Status]=N\u0027Rejected\u0027 OR [Status]=N\u0027Approved\u0027 OR [Status]=N\u0027Pending\u0027)",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "TeacherRequests",
      "name": "CK_TeacherRequests_Time",
      "sql": "([EndsAt]\u003E[StartsAt])",
      "disabled": false,
      "notTrusted": false
    },
    {
      "schema": "dbo",
      "table": "Terms",
      "name": "CK_Terms_DateRange",
      "sql": "([StartDate]\u003C=[EndDate])",
      "disabled": false,
      "notTrusted": false
    }
  ]
}');
DELETE @Documents WHERE Label='A';
INSERT @Documents VALUES ('A', (SELECT DB_NAME() AS databaseName,
    SYSUTCDATETIME() AS capturedAtUtc,
    JSON_QUERY((SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId FOR JSON PATH)) AS migrations,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table]
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
        WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name FOR JSON PATH)) AS tables,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], c.name AS [column],
        ty.name AS [type], c.max_length AS maxLengthBytes, c.precision, c.scale,
        c.is_nullable AS nullable, c.is_identity AS [identity],
        CONVERT(nvarchar(100),ic.seed_value) AS identitySeed, CONVERT(nvarchar(100),ic.increment_value) AS identityIncrement,
        c.collation_name AS collation, dc.name AS defaultName, dc.definition AS defaultSql,
        cc.definition AS computedSql, cc.is_persisted AS computedPersisted
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
        JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
        LEFT JOIN sys.computed_columns cc ON cc.object_id=c.object_id AND cc.column_id=c.column_id
        LEFT JOIN sys.identity_columns ic ON ic.object_id=c.object_id AND ic.column_id=c.column_id
        WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name,c.column_id FOR JSON PATH, INCLUDE_NULL_VALUES)) AS columns,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], i.name, i.type_desc AS [type],
        i.is_unique AS isUnique, i.is_primary_key AS isPrimaryKey, i.is_unique_constraint AS isUniqueConstraint,
        i.filter_definition AS filterSql, i.is_disabled AS disabled,
        JSON_QUERY((SELECT c.name AS [column], ic.key_ordinal AS ordinal, ic.is_descending_key AS descending,
            ic.is_included_column AS included FROM sys.index_columns ic
            JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
            WHERE ic.object_id=i.object_id AND ic.index_id=i.index_id
            ORDER BY ic.index_column_id FOR JSON PATH)) AS columns
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
        JOIN sys.indexes i ON i.object_id=t.object_id WHERE t.is_ms_shipped=0 AND i.index_id>0
        ORDER BY s.name,t.name,i.name FOR JSON PATH, INCLUDE_NULL_VALUES)) AS indexes,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], fk.name,
        OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS principalSchema,
        OBJECT_NAME(fk.referenced_object_id) AS principalTable,
        fk.delete_referential_action_desc AS onDelete, fk.update_referential_action_desc AS onUpdate,
        fk.is_disabled AS disabled, fk.is_not_trusted AS notTrusted,
        JSON_QUERY((SELECT pc.name AS [column], rc.name AS principalColumn, fc.constraint_column_id AS ordinal
            FROM sys.foreign_key_columns fc
            JOIN sys.columns pc ON pc.object_id=fc.parent_object_id AND pc.column_id=fc.parent_column_id
            JOIN sys.columns rc ON rc.object_id=fc.referenced_object_id AND rc.column_id=fc.referenced_column_id
            WHERE fc.constraint_object_id=fk.object_id ORDER BY fc.constraint_column_id FOR JSON PATH)) AS columns
        FROM sys.foreign_keys fk JOIN sys.tables t ON t.object_id=fk.parent_object_id
        JOIN sys.schemas s ON s.schema_id=t.schema_id ORDER BY s.name,t.name,fk.name FOR JSON PATH)) AS foreignKeys,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], ck.name, ck.definition AS sql,
        ck.is_disabled AS disabled, ck.is_not_trusted AS notTrusted
        FROM sys.check_constraints ck JOIN sys.tables t ON t.object_id=ck.parent_object_id
        JOIN sys.schemas s ON s.schema_id=t.schema_id ORDER BY s.name,t.name,ck.name FOR JSON PATH)) AS checks
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER));
DELETE FROM #RecoveryRows;
INSERT #RecoveryRows (Label, Kind, ObjectKey, Signature)
SELECT d.Label, bucket.[key],
    CONCAT(JSON_VALUE(obj.value,'$.schema'), N'.', JSON_VALUE(obj.value,'$.table'), N'.',
        COALESCE(JSON_VALUE(obj.value,'$.name'), JSON_VALUE(obj.value,'$.column'), N'')),
    HASHBYTES('SHA2_256', CONVERT(varbinary(max), normalized.Definition))
FROM @Documents d
CROSS APPLY OPENJSON(d.Document) bucket
CROSS APPLY OPENJSON(CASE WHEN bucket.[key] IN ('tables','columns','indexes','foreignKeys','checks') THEN bucket.value ELSE N'[]' END) obj
CROSS APPLY (
    SELECT STRING_AGG(CONVERT(nvarchar(max), CONCAT(LEN(p.[key]), N':', p.[key], N'=', p.type, N':',
        DATALENGTH(COALESCE(nested.Definition,p.value)), N':', COALESCE(nested.Definition,p.value,N'<null>'))), N'|')
        WITHIN GROUP (ORDER BY p.[key] COLLATE Latin1_General_100_BIN2) AS Definition
    FROM OPENJSON(obj.value) p
    OUTER APPLY (
        SELECT STRING_AGG(CONVERT(nvarchar(max), child.Definition), N';')
            WITHIN GROUP (ORDER BY CONVERT(int, a.[key])) AS Definition
        FROM OPENJSON(CASE WHEN p.type=4 THEN p.value ELSE N'[]' END) a
        CROSS APPLY (
            SELECT STRING_AGG(CONVERT(nvarchar(max), CONCAT(LEN(q.[key]),N':',q.[key],N'=',q.type,N':',
                DATALENGTH(q.value),N':',COALESCE(q.value,N'<null>'))), N'|')
                WITHIN GROUP (ORDER BY q.[key] COLLATE Latin1_General_100_BIN2) AS Definition
            FROM OPENJSON(a.value) q
        ) child
    ) nested
    WHERE NOT (bucket.[key]='columns' AND p.[key] IN ('defaultName','collation'))
) normalized
WHERE JSON_VALUE(obj.value,'$.table') <> '__EFMigrationsHistory';

DECLARE @isLegacy bit=0, @isTarget bit=0;
IF NOT EXISTS (SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='A' EXCEPT SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='L') AND NOT EXISTS (SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='L' EXCEPT SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='A') SET @isLegacy=1;
IF NOT EXISTS (SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='A' EXCEPT SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='T') AND NOT EXISTS (SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='T' EXCEPT SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='A') SET @isTarget=1;
IF @isLegacy=0 AND @isTarget=0 THROW 51090,N'Unsupported or incompatible schema. No recovery changes were applied.',1;
IF @isLegacy=1
BEGIN
EXEC sys.sp_executesql N'DROP INDEX [IX_MakeupEntitlements_SourceLeaveRequestId] ON [dbo].[MakeupEntitlements];
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD [LessonPassId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD [ReviewNote] nvarchar(500) NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD [ReviewedAtUtc] datetime2 NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD [ReviewedByUserId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD [RowVersion] rowversion NOT NULL;
';
EXEC sys.sp_executesql N'DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N''[dbo].[MakeupEntitlements]'') AND [c].[name] = N''SourceLeaveRequestId'');
IF @var IS NOT NULL EXEC(N''ALTER TABLE [dbo].[MakeupEntitlements] DROP CONSTRAINT '' + @var + '';'');
ALTER TABLE [dbo].[MakeupEntitlements] ALTER COLUMN [SourceLeaveRequestId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[MakeupEntitlements] ADD [SourceCancellationParticipantId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessions] ADD [CancellationReason] nvarchar(500) NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessions] ADD [CancelledAt] datetimeoffset NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessions] ADD [CancelledByUserId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessions] ADD [IsCancelled] bit NOT NULL DEFAULT CAST(0 AS bit);
';
EXEC sys.sp_executesql N'DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N''[dbo].[ClassSessionParticipants]'') AND [c].[name] = N''CourseEnrollmentId'');
IF @var1 IS NOT NULL EXEC(N''ALTER TABLE [dbo].[ClassSessionParticipants] DROP CONSTRAINT '' + @var1 + '';'');
ALTER TABLE [dbo].[ClassSessionParticipants] ALTER COLUMN [CourseEnrollmentId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD [BookingKey] uniqueidentifier NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD [CancellationReason] nvarchar(500) NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD [CancelledAtUtc] datetime2 NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD [LessonPassId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD [TrialRequestId] bigint NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[AttendanceRecords] ADD [CheckedInAtUtc] datetime2(0) NULL;
';
EXEC sys.sp_executesql N'CREATE TABLE [dbo].[SessionTeacherChanges] (
    [Id] bigint NOT NULL IDENTITY,
    [ClassSessionId] int NOT NULL,
    [PreviousTeacherId] bigint NOT NULL,
    [TeacherId] bigint NOT NULL,
    [ChangedByUserId] bigint NOT NULL,
    [ChangedAt] datetimeoffset NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_SessionTeacherChanges] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SessionTeacherChanges_ClassSessions_ClassSessionId] FOREIGN KEY ([ClassSessionId]) REFERENCES [dbo].[ClassSessions] ([Id]),
    CONSTRAINT [FK_SessionTeacherChanges_Teachers_PreviousTeacherId] FOREIGN KEY ([PreviousTeacherId]) REFERENCES [dbo].[Teachers] ([TeacherId]),
    CONSTRAINT [FK_SessionTeacherChanges_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [dbo].[Teachers] ([TeacherId]),
    CONSTRAINT [FK_SessionTeacherChanges_Users_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [dbo].[Users] ([UserId])
);
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_PortalTrialRequests_LessonPassId] ON [dbo].[PortalTrialRequests] ([LessonPassId]);
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_PortalTrialRequests_ReviewedByUserId] ON [dbo].[PortalTrialRequests] ([ReviewedByUserId]);
';
EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [IX_MakeupEntitlements_SourceCancellationParticipantId] ON [dbo].[MakeupEntitlements] ([SourceCancellationParticipantId]) WHERE [SourceCancellationParticipantId] IS NOT NULL;
';
EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [IX_MakeupEntitlements_SourceLeaveRequestId] ON [dbo].[MakeupEntitlements] ([SourceLeaveRequestId]) WHERE [SourceLeaveRequestId] IS NOT NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[MakeupEntitlements] ADD CONSTRAINT [CK_MakeupEntitlements_Source] CHECK (([SourceType]=N''Leave'' AND [SourceLeaveRequestId] IS NOT NULL AND [SourceCancellationParticipantId] IS NULL OR [SourceType]=N''Cancellation'' AND [SourceLeaveRequestId] IS NULL AND [SourceCancellationParticipantId] IS NOT NULL));
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_ClassSessions_CancelledByUserId] ON [dbo].[ClassSessions] ([CancelledByUserId]);
';
EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [IX_ClassSessionParticipants_BookingKey] ON [dbo].[ClassSessionParticipants] ([BookingKey]) WHERE [BookingKey] IS NOT NULL;
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_ClassSessionParticipants_LessonPassId] ON [dbo].[ClassSessionParticipants] ([LessonPassId]);
';
EXEC sys.sp_executesql N'CREATE UNIQUE INDEX [IX_ClassSessionParticipants_TrialRequestId] ON [dbo].[ClassSessionParticipants] ([TrialRequestId]) WHERE [TrialRequestId] IS NOT NULL;
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD CONSTRAINT [CK_ClassSessionParticipants_Source] CHECK (([ParticipationType]=N''Regular'' AND [CourseEnrollmentId] IS NOT NULL AND [LessonPassId] IS NULL OR [ParticipationType]=N''Makeup'' AND [MakeupRequestId] IS NOT NULL AND [LessonPassId] IS NULL OR [ParticipationType]=N''Trial'' AND [CourseEnrollmentId] IS NULL AND [LessonPassId] IS NOT NULL AND [TrialRequestId] IS NOT NULL OR [ParticipationType]=N''Flexible'' AND [CourseEnrollmentId] IS NULL AND [LessonPassId] IS NOT NULL AND [BookingKey] IS NOT NULL));
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_SessionTeacherChanges_ChangedByUserId] ON [dbo].[SessionTeacherChanges] ([ChangedByUserId]);
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_SessionTeacherChanges_ClassSessionId] ON [dbo].[SessionTeacherChanges] ([ClassSessionId]);
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_SessionTeacherChanges_PreviousTeacherId] ON [dbo].[SessionTeacherChanges] ([PreviousTeacherId]);
';
EXEC sys.sp_executesql N'CREATE INDEX [IX_SessionTeacherChanges_TeacherId] ON [dbo].[SessionTeacherChanges] ([TeacherId]);
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD CONSTRAINT [FK_ClassSessionParticipants_LessonPasses_LessonPassId] FOREIGN KEY ([LessonPassId]) REFERENCES [dbo].[LessonPasses] ([LessonPassId]);
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessionParticipants] ADD CONSTRAINT [FK_ClassSessionParticipants_PortalTrialRequests_TrialRequestId] FOREIGN KEY ([TrialRequestId]) REFERENCES [dbo].[PortalTrialRequests] ([Id]);
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[ClassSessions] ADD CONSTRAINT [FK_ClassSessions_Users_CancelledByUserId] FOREIGN KEY ([CancelledByUserId]) REFERENCES [dbo].[Users] ([UserId]);
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[MakeupEntitlements] ADD CONSTRAINT [FK_MakeupEntitlements_ClassSessionParticipants_SourceCancellationParticipantId] FOREIGN KEY ([SourceCancellationParticipantId]) REFERENCES [dbo].[ClassSessionParticipants] ([SessionParticipantId]);
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD CONSTRAINT [FK_PortalTrialRequests_LessonPasses_LessonPassId] FOREIGN KEY ([LessonPassId]) REFERENCES [dbo].[LessonPasses] ([LessonPassId]);
';
EXEC sys.sp_executesql N'ALTER TABLE [dbo].[PortalTrialRequests] ADD CONSTRAINT [FK_PortalTrialRequests_Users_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [dbo].[Users] ([UserId]);
';
END;
DELETE @Documents WHERE Label='A';
INSERT @Documents VALUES ('A', (SELECT DB_NAME() AS databaseName,
    SYSUTCDATETIME() AS capturedAtUtc,
    JSON_QUERY((SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId FOR JSON PATH)) AS migrations,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table]
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
        WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name FOR JSON PATH)) AS tables,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], c.name AS [column],
        ty.name AS [type], c.max_length AS maxLengthBytes, c.precision, c.scale,
        c.is_nullable AS nullable, c.is_identity AS [identity],
        CONVERT(nvarchar(100),ic.seed_value) AS identitySeed, CONVERT(nvarchar(100),ic.increment_value) AS identityIncrement,
        c.collation_name AS collation, dc.name AS defaultName, dc.definition AS defaultSql,
        cc.definition AS computedSql, cc.is_persisted AS computedPersisted
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
        JOIN sys.columns c ON c.object_id=t.object_id JOIN sys.types ty ON ty.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
        LEFT JOIN sys.computed_columns cc ON cc.object_id=c.object_id AND cc.column_id=c.column_id
        LEFT JOIN sys.identity_columns ic ON ic.object_id=c.object_id AND ic.column_id=c.column_id
        WHERE t.is_ms_shipped=0 ORDER BY s.name,t.name,c.column_id FOR JSON PATH, INCLUDE_NULL_VALUES)) AS columns,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], i.name, i.type_desc AS [type],
        i.is_unique AS isUnique, i.is_primary_key AS isPrimaryKey, i.is_unique_constraint AS isUniqueConstraint,
        i.filter_definition AS filterSql, i.is_disabled AS disabled,
        JSON_QUERY((SELECT c.name AS [column], ic.key_ordinal AS ordinal, ic.is_descending_key AS descending,
            ic.is_included_column AS included FROM sys.index_columns ic
            JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
            WHERE ic.object_id=i.object_id AND ic.index_id=i.index_id
            ORDER BY ic.index_column_id FOR JSON PATH)) AS columns
        FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id
        JOIN sys.indexes i ON i.object_id=t.object_id WHERE t.is_ms_shipped=0 AND i.index_id>0
        ORDER BY s.name,t.name,i.name FOR JSON PATH, INCLUDE_NULL_VALUES)) AS indexes,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], fk.name,
        OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS principalSchema,
        OBJECT_NAME(fk.referenced_object_id) AS principalTable,
        fk.delete_referential_action_desc AS onDelete, fk.update_referential_action_desc AS onUpdate,
        fk.is_disabled AS disabled, fk.is_not_trusted AS notTrusted,
        JSON_QUERY((SELECT pc.name AS [column], rc.name AS principalColumn, fc.constraint_column_id AS ordinal
            FROM sys.foreign_key_columns fc
            JOIN sys.columns pc ON pc.object_id=fc.parent_object_id AND pc.column_id=fc.parent_column_id
            JOIN sys.columns rc ON rc.object_id=fc.referenced_object_id AND rc.column_id=fc.referenced_column_id
            WHERE fc.constraint_object_id=fk.object_id ORDER BY fc.constraint_column_id FOR JSON PATH)) AS columns
        FROM sys.foreign_keys fk JOIN sys.tables t ON t.object_id=fk.parent_object_id
        JOIN sys.schemas s ON s.schema_id=t.schema_id ORDER BY s.name,t.name,fk.name FOR JSON PATH)) AS foreignKeys,
    JSON_QUERY((SELECT s.name AS [schema], t.name AS [table], ck.name, ck.definition AS sql,
        ck.is_disabled AS disabled, ck.is_not_trusted AS notTrusted
        FROM sys.check_constraints ck JOIN sys.tables t ON t.object_id=ck.parent_object_id
        JOIN sys.schemas s ON s.schema_id=t.schema_id ORDER BY s.name,t.name,ck.name FOR JSON PATH)) AS checks
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER));
DELETE FROM #RecoveryRows;
INSERT #RecoveryRows (Label, Kind, ObjectKey, Signature)
SELECT d.Label, bucket.[key],
    CONCAT(JSON_VALUE(obj.value,'$.schema'), N'.', JSON_VALUE(obj.value,'$.table'), N'.',
        COALESCE(JSON_VALUE(obj.value,'$.name'), JSON_VALUE(obj.value,'$.column'), N'')),
    HASHBYTES('SHA2_256', CONVERT(varbinary(max), normalized.Definition))
FROM @Documents d
CROSS APPLY OPENJSON(d.Document) bucket
CROSS APPLY OPENJSON(CASE WHEN bucket.[key] IN ('tables','columns','indexes','foreignKeys','checks') THEN bucket.value ELSE N'[]' END) obj
CROSS APPLY (
    SELECT STRING_AGG(CONVERT(nvarchar(max), CONCAT(LEN(p.[key]), N':', p.[key], N'=', p.type, N':',
        DATALENGTH(COALESCE(nested.Definition,p.value)), N':', COALESCE(nested.Definition,p.value,N'<null>'))), N'|')
        WITHIN GROUP (ORDER BY p.[key] COLLATE Latin1_General_100_BIN2) AS Definition
    FROM OPENJSON(obj.value) p
    OUTER APPLY (
        SELECT STRING_AGG(CONVERT(nvarchar(max), child.Definition), N';')
            WITHIN GROUP (ORDER BY CONVERT(int, a.[key])) AS Definition
        FROM OPENJSON(CASE WHEN p.type=4 THEN p.value ELSE N'[]' END) a
        CROSS APPLY (
            SELECT STRING_AGG(CONVERT(nvarchar(max), CONCAT(LEN(q.[key]),N':',q.[key],N'=',q.type,N':',
                DATALENGTH(q.value),N':',COALESCE(q.value,N'<null>'))), N'|')
                WITHIN GROUP (ORDER BY q.[key] COLLATE Latin1_General_100_BIN2) AS Definition
            FROM OPENJSON(a.value) q
        ) child
    ) nested
    WHERE NOT (bucket.[key]='columns' AND p.[key] IN ('defaultName','collation'))
) normalized
WHERE JSON_VALUE(obj.value,'$.table') <> '__EFMigrationsHistory';

IF NOT (NOT EXISTS (SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='A' EXCEPT SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='T') AND NOT EXISTS (SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='T' EXCEPT SELECT Kind,ObjectKey,Signature FROM #RecoveryRows WHERE Label='A')) THROW 51090,N'Recovery target verification failed. Roll back this transaction.',1;
DROP TABLE #RecoveryRows;