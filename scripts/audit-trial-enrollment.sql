-- Read-only inventory. Run against the same database as EnjoySpace.Web.
-- No INSERT/UPDATE/DELETE, schema changes, or personally identifying values.
SET NOCOUNT ON;

SELECT DB_NAME() AS DatabaseName, SYSUTCDATETIME() AS CheckedAtUtc;
SELECT MigrationId AS AppliedMigration
FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;

-- A. Existing statuses are evidence; never reinterpret Contacted as approved.
SELECT N'PortalTrialRequests' AS Entity, Status, COUNT_BIG(*) AS RecordCount
FROM dbo.PortalTrialRequests GROUP BY Status
UNION ALL
SELECT N'TrialGuests', Status, COUNT_BIG(*) FROM dbo.TrialGuests GROUP BY Status
UNION ALL
SELECT N'LessonPasses', Status, COUNT_BIG(*) FROM dbo.LessonPasses GROUP BY Status
UNION ALL
SELECT N'CourseEnrollments', Status, COUNT_BIG(*) FROM dbo.CourseEnrollments GROUP BY Status
UNION ALL
SELECT N'EnrollmentApplications', Status, COUNT_BIG(*) FROM dbo.EnrollmentApplications GROUP BY Status
UNION ALL
SELECT N'ClassSessionParticipants', Status, COUNT_BIG(*) FROM dbo.ClassSessionParticipants GROUP BY Status
UNION ALL
SELECT N'AttendanceRecords', Status, COUNT_BIG(*) FROM dbo.AttendanceRecords GROUP BY Status
ORDER BY Entity, Status;

-- B. Counts only; do not disclose names, contacts, passwords, or account names.
SELECT N'TotalTrialRequests' AS CheckName, COUNT_BIG(*) AS RecordCount FROM dbo.PortalTrialRequests
UNION ALL
SELECT N'AccountsWithMultiplePendingOrContactedRequests', COUNT_BIG(*) FROM (
    SELECT UserId FROM dbo.PortalTrialRequests WHERE Status IN (N'Pending', N'Contacted')
    GROUP BY UserId HAVING COUNT_BIG(*) > 1
) AS DuplicateRequests
UNION ALL
SELECT N'GuestsLinkedToMultipleRequestAccounts', COUNT_BIG(*) FROM (
    SELECT TrialGuestId FROM dbo.PortalTrialRequests GROUP BY TrialGuestId HAVING COUNT(DISTINCT UserId) > 1
) AS SharedGuests
UNION ALL
SELECT N'GuestsWithoutRequestOrConvertedStudentLink', COUNT_BIG(*) FROM dbo.TrialGuests g
WHERE g.ConvertedStudentId IS NULL AND NOT EXISTS (SELECT 1 FROM dbo.PortalTrialRequests r WHERE r.TrialGuestId = g.TrialGuestId)
UNION ALL
SELECT N'TrialPassesWithoutTraceableAccount', COUNT_BIG(*) FROM dbo.LessonPasses p
JOIN dbo.TrialGuests g ON g.TrialGuestId = p.TrialGuestId
WHERE p.PlanTypeAtPurchase = N'Trial' AND g.ConvertedStudentId IS NULL
AND NOT EXISTS (SELECT 1 FROM dbo.PortalTrialRequests r WHERE r.TrialGuestId = g.TrialGuestId)
UNION ALL
SELECT N'PendingOrContactedRequestsWithFormalEnrollmentHistory', COUNT_BIG(*) FROM dbo.PortalTrialRequests r
WHERE r.Status IN (N'Pending', N'Contacted') AND EXISTS (
    SELECT 1 FROM dbo.Students s JOIN dbo.CourseEnrollments e ON e.StudentId = s.StudentId WHERE s.UserId = r.UserId
)
UNION ALL
SELECT N'ApprovedApplicationsWithoutFormalEnrollment', COUNT_BIG(*) FROM dbo.EnrollmentApplications a
WHERE a.Status = N'Approved' AND NOT EXISTS (SELECT 1 FROM dbo.CourseEnrollments e WHERE e.EnrollmentApplicationId = a.EnrollmentApplicationId)
UNION ALL
SELECT N'FormalEnrollmentsWithNonApprovedApplications', COUNT_BIG(*) FROM dbo.CourseEnrollments e
JOIN dbo.EnrollmentApplications a ON a.EnrollmentApplicationId = e.EnrollmentApplicationId WHERE a.Status <> N'Approved'
UNION ALL
SELECT N'RequestsMissingRequiredRelatedRecords', COUNT_BIG(*) FROM dbo.PortalTrialRequests r
LEFT JOIN dbo.Users u ON u.UserId = r.UserId
LEFT JOIN dbo.TrialGuests g ON g.TrialGuestId = r.TrialGuestId
LEFT JOIN dbo.ClassSessions s ON s.Id = r.ClassSessionId
LEFT JOIN dbo.LessonPlans p ON p.LessonPlanId = r.LessonPlanId
WHERE u.UserId IS NULL OR g.TrialGuestId IS NULL OR s.Id IS NULL OR p.LessonPlanId IS NULL;

-- C. The actual schema may differ from code. Check nullability and available links.
SELECT CONVERT(nvarchar(40), t.name) AS TableName, CONVERT(nvarchar(40), c.name) AS ColumnName,
    CONVERT(nvarchar(20), TYPE_NAME(c.user_type_id)) AS DataType, c.is_nullable AS IsNullable
FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
WHERE s.name = N'dbo' AND t.name IN
    (N'PortalTrialRequests', N'ClassSessionParticipants', N'ClassSessions', N'LessonPasses')
ORDER BY t.name, c.column_id;

-- D. Existing roster categories and database constraints, without personal values.
SELECT p.ParticipationType, a.Status AS AttendanceStatus, COUNT_BIG(*) AS RecordCount
FROM dbo.ClassSessionParticipants p LEFT JOIN dbo.AttendanceRecords a ON a.SessionParticipantId = p.SessionParticipantId
GROUP BY p.ParticipationType, a.Status ORDER BY p.ParticipationType, a.Status;

SELECT CONVERT(nvarchar(40), OBJECT_NAME(parent_object_id)) AS TableName, name AS ConstraintName, definition
FROM sys.check_constraints WHERE parent_object_id IN
    (OBJECT_ID(N'dbo.PortalTrialRequests'), OBJECT_ID(N'dbo.ClassSessionParticipants'));

SELECT CONVERT(nvarchar(40), OBJECT_NAME(fk.parent_object_id)) AS TableName,
    CONVERT(nvarchar(40), pc.name) AS ColumnName,
    CONVERT(nvarchar(40), OBJECT_NAME(fk.referenced_object_id)) AS ReferencedTable,
    CONVERT(nvarchar(40), rc.name) AS ReferencedColumn, fk.delete_referential_action_desc AS OnDelete
FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fc ON fc.constraint_object_id = fk.object_id
JOIN sys.columns pc ON pc.object_id = fc.parent_object_id AND pc.column_id = fc.parent_column_id
JOIN sys.columns rc ON rc.object_id = fc.referenced_object_id AND rc.column_id = fc.referenced_column_id
WHERE fk.parent_object_id IN (OBJECT_ID(N'dbo.PortalTrialRequests'), OBJECT_ID(N'dbo.ClassSessionParticipants'))
ORDER BY TableName, ColumnName;
