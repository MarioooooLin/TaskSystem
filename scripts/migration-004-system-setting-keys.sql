-- ================================================================
-- Migration 004: 補齊系統參數 Key 並修正命名
-- 目標：將舊 key `kol_payout_min_amount` 更名為 `kol_min_payout_amount`，
--       並補齊後台參數設定頁所需但資料庫缺少的 Key。
-- 特性：idempotent，可重複執行；已存在或已正確的資料不會被覆寫。
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
-- 1. 修正舊的 key 名稱
IF EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_payout_min_amount'
)
AND NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_min_payout_amount'
) BEGIN
UPDATE SystemSettings
SET [Key] = 'kol_min_payout_amount'
WHERE [Key] = 'kol_payout_min_amount';
END -- 2. 補齊缺少的系統參數
IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_min_payout_amount'
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
        'kol_min_payout_amount',
        '1000',
        '1000',
        'number',
        'payout',
        N'KOL 最低提領門檻；金額需 >= 此值才可提領'
    );
END IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_tax_rate'
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
        'kol_tax_rate',
        '0',
        '0',
        'percent',
        'payout',
        N'KOL 稅金扣除率'
    );
END IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_payout_fee_rate'
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
        'kol_payout_fee_rate',
        '0',
        '0',
        'percent',
        'payout',
        N'KOL 提領手續費率'
    );
END IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_payout_fixed_fee_amount'
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
        'kol_payout_fixed_fee_amount',
        '0',
        '0',
        'number',
        'payout',
        N'KOL 提領固定手續費'
    );
END IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_payout_mode'
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
        'kol_payout_mode',
        N'全額提領',
        N'全額提領',
        'string',
        'payout',
        N'提領方式'
    );
END IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_payout_days'
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
        'kol_payout_days',
        '10,25',
        '10,25',
        'string',
        'payout',
        N'撥款日'
    );
END IF NOT EXISTS (
    SELECT 1
    FROM SystemSettings
    WHERE [Key] = 'kol_payout_closing_day_offset'
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
        'kol_payout_closing_day_offset',
        '-5',
        '-5',
        'number',
        'payout',
        N'關帳日設定'
    );
END -- 3. 補齊既有參數的 DefaultValue，避免「回復預設」後清空數值
UPDATE s
SET DefaultValue = d.DefaultValue
FROM SystemSettings s
    INNER JOIN (
        VALUES ('case_opening_fee_amount', '1000'),
            ('platform_service_fee_rate', '0'),
            ('affiliate_platform_commission_rate', '0'),
            ('affiliate_kol_min_commission_rate', '0'),
            ('case_auto_execution_threshold_rate', '50'),
            ('case_reconfirmation_deadline_days', '3'),
            ('kol_min_payout_amount', '1000'),
            ('kol_tax_rate', '0'),
            ('kol_payout_fee_rate', '0'),
            ('kol_payout_fixed_fee_amount', '0'),
            ('kol_payout_mode', N'全額提領'),
            ('kol_payout_days', '10,25'),
            ('kol_payout_closing_day_offset', '-5')
    ) AS d([Key], DefaultValue) ON s.[Key] = d.[Key]
WHERE NULLIF(s.DefaultValue, '') IS NULL;
COMMIT TRANSACTION;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;