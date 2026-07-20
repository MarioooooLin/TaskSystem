-- ================================================================
-- Migration 003: 業者端新增/編輯案件與發布確認 (MER-003 / MER-004)
-- 目標：調整 Cases / CaseLanguages / CasePlatforms / CaseCategories
--       與 CaseBudgetSnapshots 欄位，並安全遷移既有資料。
-- 特性：idempotent，可重複執行；已正確資料不會被覆寫或重複遷移。
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
-- ================================================================
-- 1. 系統參數
-- ================================================================
IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'case_reconfirmation_deadline_days'
) BEGIN
INSERT INTO SystemSettings (
        [Key],
        Value,
        DefaultValue,
        ValueType,
        [Group],
        Description
    )
VALUES (
        'case_reconfirmation_deadline_days',
        '3',
        '3',
        'number',
        'case',
        N'招募中案件修改後，已錄取 KOL 重新確認期限（日曆天）'
    );
END -- ================================================================
-- 2. Cases 欄位 nullable 調整（草稿允許不完整）
-- ================================================================
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'Title'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN Title NVARCHAR(200) NULL;
END IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'Description'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN Description NVARCHAR(MAX) NULL;
END IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'City'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN City NVARCHAR(100) NULL;
END IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'Address'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN Address NVARCHAR(300) NULL;
END IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'ApplicationDeadline'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN ApplicationDeadline DATETIME2 NULL;
END IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'SubmissionDeadline'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN SubmissionDeadline DATETIME2 NULL;
END IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'DeliverableDescription'
        AND is_nullable = 0
) BEGIN
ALTER TABLE Cases
ALTER COLUMN DeliverableDescription NVARCHAR(MAX) NULL;
END IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Cases')
        AND name = 'WantedKolCount'
        AND default_definition IS NOT NULL
) BEGIN
ALTER TABLE Cases
ADD CONSTRAINT DF_Cases_WantedKolCount DEFAULT 0 FOR WantedKolCount;
END -- ================================================================
-- 3. CaseLanguages：SMALLINT -> LanguageCode NVARCHAR(50)
-- ================================================================
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('CaseLanguages')
        AND name = 'Language'
        AND system_type_id = TYPE_ID('smallint')
) BEGIN -- 先安全遷移既有數值
IF EXISTS (
    SELECT 1
    FROM CaseLanguages
) BEGIN
UPDATE cl
SET Language = CASE
        cl.Language
        WHEN 1 THEN N'zh-TW'
        WHEN 2 THEN N'en'
        WHEN 3 THEN N'ja'
        WHEN 4 THEN N'ko'
        ELSE N'zh-TW'
    END
FROM CaseLanguages cl;
END -- 修改欄位型別與名稱
ALTER TABLE CaseLanguages
ALTER COLUMN Language NVARCHAR(50) NOT NULL;
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('CaseLanguages')
        AND name = 'LanguageCode'
) BEGIN EXEC sp_rename 'CaseLanguages.Language',
'LanguageCode',
'COLUMN';
END
END -- 新增 nan-TW 到既有 zh-TW 為主的多語案件
IF EXISTS (
    SELECT 1
    FROM CaseLanguages
    WHERE LanguageCode = N'zh-TW'
)
AND NOT EXISTS (
    SELECT 1
    FROM CaseLanguages
    WHERE LanguageCode = N'nan-TW'
) BEGIN
INSERT INTO CaseLanguages (CaseId, LanguageCode)
SELECT DISTINCT CaseId,
    N'nan-TW'
FROM CaseLanguages
WHERE LanguageCode = N'zh-TW';
END -- ================================================================
-- 4. CasePlatforms：舊 1~6 映射到新 SocialPlatform 1~11
-- ================================================================
IF EXISTS (
    SELECT 1
    FROM CasePlatforms
) BEGIN
UPDATE cp
SET Platform = CASE
        cp.Platform
        WHEN 1 THEN 2 -- Instagram -> 2
        WHEN 2 THEN 3 -- Facebook -> 3
        WHEN 3 THEN 4 -- YouTube -> 4
        WHEN 4 THEN 7 -- TikTok -> 7
        WHEN 5 THEN 9 -- Threads -> 9
        WHEN 6 THEN 5 -- Blog -> 5
        ELSE cp.Platform
    END
FROM CasePlatforms cp
WHERE cp.Platform BETWEEN 1 AND 6;
END IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_CasePlatforms_Plat'
) BEGIN
ALTER TABLE CasePlatforms DROP CONSTRAINT CK_CasePlatforms_Plat;
END
ALTER TABLE CasePlatforms
ADD CONSTRAINT CK_CasePlatforms_Plat CHECK (Platform IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11));
-- ================================================================
-- 5. CaseCategories：補上 CHECK 並對齊 KolCategories
-- ================================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_CaseCategories_Category'
) BEGIN
ALTER TABLE CaseCategories
ADD CONSTRAINT CK_CaseCategories_Category CHECK (
        Category BETWEEN 1 AND 26
    );
END -- ================================================================
-- 6. CaseBudgetSnapshots：新增 IdempotencyKey 與唯一索引
-- ================================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('CaseBudgetSnapshots')
        AND name = 'IdempotencyKey'
) BEGIN
ALTER TABLE CaseBudgetSnapshots
ADD IdempotencyKey NVARCHAR(100) NULL;
END IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_CaseBudgetSnapshots_IdempotencyKey'
) BEGIN CREATE UNIQUE INDEX UX_CaseBudgetSnapshots_IdempotencyKey ON CaseBudgetSnapshots (IdempotencyKey)
WHERE IdempotencyKey IS NOT NULL;
END -- ================================================================
-- 7. 權限 Seed：Merchant.Case.View / Manage / Publish
-- ================================================================
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Merchant.Case.View'
)
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Merchant.Case.View', N'查看案件', 1);
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Merchant.Case.Manage'
)
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Merchant.Case.Manage', N'新增與編輯案件', 1);
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Merchant.Case.Publish'
)
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Merchant.Case.Publish', N'發布案件並鎖定預算', 2);
COMMIT TRANSACTION;
SELECT N'Migration 003 完成：案件編輯/發布相關 Schema 與權限已更新。' AS Result;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;