-- 直接修正 CaseLanguages：將舊的 Language 欄位轉為 LanguageCode NVARCHAR(50)
-- 執行後即可解決「無效的資料行名稱 'LanguageCode'」錯誤
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
-- 1. 如果已經存在 LanguageCode，就不動
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('CaseLanguages')
        AND name = 'LanguageCode'
) BEGIN -- 2. 如果存在舊的 Language 欄位：先轉型再改名
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('CaseLanguages')
        AND name = 'Language'
) BEGIN -- 新增中繼 NVARCHAR 欄位
ALTER TABLE CaseLanguages
ADD LanguageCode_Temp NVARCHAR(50) NULL;
-- 把舊值轉成語言碼字串（先判斷是否還是 smallint）
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('CaseLanguages')
        AND name = 'Language'
        AND system_type_id = TYPE_ID('smallint')
) BEGIN
UPDATE CaseLanguages
SET LanguageCode_Temp = CASE
        Language
        WHEN 1 THEN N'zh-TW'
        WHEN 2 THEN N'en'
        WHEN 3 THEN N'ja'
        WHEN 4 THEN N'ko'
        ELSE N'zh-TW'
    END;
END
ELSE BEGIN
UPDATE CaseLanguages
SET LanguageCode_Temp = CAST(Language AS NVARCHAR(50));
END -- 刪除舊欄位、重命名中繼欄位
ALTER TABLE CaseLanguages DROP COLUMN Language;
EXEC sp_rename 'CaseLanguages.LanguageCode_Temp',
'LanguageCode',
'COLUMN';
ALTER TABLE CaseLanguages
ALTER COLUMN LanguageCode NVARCHAR(50) NOT NULL;
END
ELSE BEGIN -- 3. 如果兩個欄位都沒有，直接新增 LanguageCode
ALTER TABLE CaseLanguages
ADD LanguageCode NVARCHAR(50) NOT NULL DEFAULT N'zh-TW';
END
END COMMIT TRANSACTION;
SELECT COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CaseLanguages'
ORDER BY ORDINAL_POSITION;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;