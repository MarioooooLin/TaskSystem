-- ================================================================
-- Migration: 建立 Languages 語言字典表
-- 用途：統一管理 KOL 語言條件與未來系統多語系支援
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
IF OBJECT_ID('dbo.Languages', 'U') IS NULL BEGIN CREATE TABLE Languages (
    Code NVARCHAR(50) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    SortOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_Languages PRIMARY KEY (Code)
);
END -- 種子資料：與 KolLanguages.LanguageCode 慣例一致
MERGE INTO Languages AS target USING (
    VALUES (N'zh-TW', N'中文', 1, 1),
        (N'nan-TW', N'台語', 1, 2),
        (N'en', N'英文', 1, 3),
        (N'ja', N'日文', 1, 4),
        (N'ko', N'韓文', 1, 5)
) AS source (Code, DisplayName, IsActive, SortOrder) ON target.Code = source.Code
WHEN MATCHED THEN
UPDATE
SET DisplayName = source.DisplayName,
    IsActive = source.IsActive,
    SortOrder = source.SortOrder
    WHEN NOT MATCHED THEN
INSERT (Code, DisplayName, IsActive, SortOrder)
VALUES (
        source.Code,
        source.DisplayName,
        source.IsActive,
        source.SortOrder
    );
COMMIT TRANSACTION;
SELECT Code,
    DisplayName,
    IsActive,
    SortOrder
FROM Languages
ORDER BY SortOrder;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;