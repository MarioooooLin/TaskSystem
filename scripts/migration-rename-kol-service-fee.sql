-- ================================================================
-- Migration: 將 kol_service_fee_rate 重命名為 platform_service_fee_rate
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
-- 1. 更新 SystemSettings 的 Key
IF EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_service_fee_rate'
) BEGIN
UPDATE SystemSettings
SET [Key] = 'platform_service_fee_rate',
    Description = N'平台服務費率；案件發布預估凍結金額使用'
WHERE [Key] = 'kol_service_fee_rate';
END -- 2. 更新 CaseBudgetSnapshots 裡的 SettingsSnapshot JSON key
IF EXISTS (
    SELECT 1
    FROM CaseBudgetSnapshots
    WHERE SettingsSnapshot LIKE '%"kol_service_fee_rate"%'
) BEGIN
UPDATE CaseBudgetSnapshots
SET SettingsSnapshot = REPLACE(
        SettingsSnapshot,
        '"kol_service_fee_rate"',
        '"platform_service_fee_rate"'
    )
WHERE SettingsSnapshot LIKE '%"kol_service_fee_rate"%';
END COMMIT TRANSACTION;
SELECT N'kol_service_fee_rate 已成功重命名為 platform_service_fee_rate' AS Result;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH