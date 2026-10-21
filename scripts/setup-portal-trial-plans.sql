-- User-approved portal plans. Run only after AddPortalTrialReferrals and SupportMonthlyLessonPlans.
-- Existing matching plans are preserved; conflicting codes abort rather than overwrite admin edits.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF EXISTS (SELECT 1 FROM dbo.LessonPlans WITH (UPDLOCK, HOLDLOCK) WHERE PlanCode = N'TRIAL-REFERRAL-1'
    AND (PlanName <> N'舊生介紹一堂免費' OR PlanType <> N'Trial' OR AudienceType <> N'TrialGuest' OR SessionQuota <> 1 OR Price <> 0 OR RequiresReferrer <> 1 OR IsActive <> 1 OR ValidityUnit <> N'Unlimited' OR ValidityValue IS NOT NULL))
    THROW 51000, N'介紹方案代碼已有不同設定，請人工確認。', 1;
IF EXISTS (SELECT 1 FROM dbo.LessonPlans WITH (UPDLOCK, HOLDLOCK) WHERE PlanCode = N'TRIAL-3-2MONTHS'
    AND (PlanName <> N'三堂體驗課' OR PlanType <> N'Trial' OR AudienceType <> N'TrialGuest' OR SessionQuota <> 3 OR Price <> 1000 OR RequiresReferrer <> 0 OR IsActive <> 1 OR ValidityUnit <> N'Months' OR ISNULL(ValidityValue, 0) <> 2))
    THROW 51001, N'三堂方案代碼已有不同設定，請人工確認。', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.LessonPlans WHERE PlanCode = N'TRIAL-REFERRAL-1')
    INSERT dbo.LessonPlans (PlanCode, PlanName, PlanType, AudienceType, SessionQuota, Price, ValidityUnit, ValidityValue, IsActive, RequiresReferrer, CreatedAtUtc)
    VALUES (N'TRIAL-REFERRAL-1', N'舊生介紹一堂免費', N'Trial', N'TrialGuest', 1, 0, N'Unlimited', NULL, 1, 1, SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM dbo.LessonPlans WHERE PlanCode = N'TRIAL-3-2MONTHS')
    INSERT dbo.LessonPlans (PlanCode, PlanName, PlanType, AudienceType, SessionQuota, Price, ValidityUnit, ValidityValue, IsActive, RequiresReferrer, CreatedAtUtc)
    VALUES (N'TRIAL-3-2MONTHS', N'三堂體驗課', N'Trial', N'TrialGuest', 3, 1000, N'Months', 2, 1, 0, SYSUTCDATETIME());
COMMIT;
SELECT PlanCode, PlanName, SessionQuota, Price, ValidityUnit, ValidityValue, RequiresReferrer, IsActive
FROM dbo.LessonPlans WHERE PlanCode IN (N'TRIAL-REFERRAL-1', N'TRIAL-3-2MONTHS');
