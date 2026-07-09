-- Seed one case for every Cases.Status value.
-- Safe to run repeatedly: MerchantId + Title is used as the seed key.

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @MerchantId BIGINT;
DECLARE @CreatedByUserId BIGINT;
DECLARE @Now DATETIME2 = GETUTCDATE();

SELECT TOP (1)
    @MerchantId = m.Id,
    @CreatedByUserId = m.UserId
FROM Merchants AS m
INNER JOIN Users AS u ON u.Id = m.UserId
WHERE m.VerificationStatus = 2
  AND u.Status = 1
ORDER BY m.Id;

IF @MerchantId IS NULL OR @CreatedByUserId IS NULL
    THROW 50001, N'找不到已通過審核且帳號啟用中的業者，請先執行業者 seed。', 1;

DECLARE @CaseSeeds TABLE (
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    WantedKolCount INT NOT NULL,
    CashRewardAmount DECIMAL(12, 2) NOT NULL,
    Status SMALLINT NOT NULL,
    RecruitmentStatus SMALLINT NOT NULL,
    ApplicationDeadline DATETIME2 NOT NULL,
    SubmissionDeadline DATETIME2 NOT NULL,
    PublishedAt DATETIME2 NULL,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    SettledAt DATETIME2 NULL,
    CancelledAt DATETIME2 NULL
);

INSERT INTO @CaseSeeds (
    Title,
    Description,
    City,
    Address,
    WantedKolCount,
    CashRewardAmount,
    Status,
    RecruitmentStatus,
    ApplicationDeadline,
    SubmissionDeadline,
    PublishedAt,
    StartedAt,
    CompletedAt,
    SettledAt,
    CancelledAt
)
VALUES
    (
        N'[SEED] 花蓮山海民宿開箱',
        N'提供花蓮民宿住宿體驗，案件仍在草稿階段。',
        N'花蓮縣',
        N'花蓮市海岸路 10 號',
        2,
        6000.00,
        1, -- Draft
        1, -- NotOpen
        DATEADD(DAY, 21, @Now),
        DATEADD(DAY, 40, @Now),
        NULL, NULL, NULL, NULL, NULL
    ),
    (
        N'[SEED] 台北城市旅宿體驗',
        N'邀請旅遊與生活風格 KOL 體驗台北市區旅宿，分享住宿空間、早餐與周邊散步路線。',
        N'台北市',
        N'中山區南京東路一段 100 號',
        3,
        8000.00,
        2, -- Recruiting
        2, -- Open
        DATEADD(DAY, 14, @Now),
        DATEADD(DAY, 30, @Now),
        @Now, NULL, NULL, NULL, NULL
    ),
    (
        N'[SEED] 台南老宅文化散策',
        N'招募已截止，等待業者確認錄取名單。',
        N'台南市',
        N'中西區府前路二段 20 號',
        3,
        7500.00,
        3, -- RecruitmentClosed
        3, -- Closed
        DATEADD(DAY, -2, @Now),
        DATEADD(DAY, 14, @Now),
        DATEADD(DAY, -20, @Now), NULL, NULL, NULL, NULL
    ),
    (
        N'[SEED] 墾丁海景度假企劃',
        N'案件執行中，KOL 正在體驗並準備交付內容。',
        N'屏東縣',
        N'恆春鎮墾丁路 88 號',
        4,
        10000.00,
        4, -- InProgress
        3, -- Closed
        DATEADD(DAY, -15, @Now),
        DATEADD(DAY, 10, @Now),
        DATEADD(DAY, -30, @Now),
        DATEADD(DAY, -7, @Now),
        NULL, NULL, NULL
    ),
    (
        N'[SEED] 宜蘭溫泉住宿分享',
        N'所有任務均已完成，等待案件結算。',
        N'宜蘭縣',
        N'礁溪鄉溫泉路 66 號',
        2,
        9000.00,
        5, -- Completed
        3, -- Closed
        DATEADD(DAY, -45, @Now),
        DATEADD(DAY, -10, @Now),
        DATEADD(DAY, -60, @Now),
        DATEADD(DAY, -35, @Now),
        DATEADD(DAY, -3, @Now),
        NULL, NULL
    ),
    (
        N'[SEED] 日月潭湖畔旅行',
        N'案件與相關款項均已完成結算。',
        N'南投縣',
        N'魚池鄉中山路 168 號',
        3,
        8500.00,
        6, -- Settled
        3, -- Closed
        DATEADD(DAY, -75, @Now),
        DATEADD(DAY, -40, @Now),
        DATEADD(DAY, -90, @Now),
        DATEADD(DAY, -65, @Now),
        DATEADD(DAY, -30, @Now),
        DATEADD(DAY, -20, @Now),
        NULL
    ),
    (
        N'[SEED] 高雄港灣美食企劃',
        N'因活動檔期調整，業者已取消此案件。',
        N'高雄市',
        N'鹽埕區大勇路 1 號',
        5,
        5000.00,
        7, -- Cancelled
        3, -- Closed
        DATEADD(DAY, 5, @Now),
        DATEADD(DAY, 20, @Now),
        DATEADD(DAY, -10, @Now),
        NULL, NULL, NULL,
        DATEADD(DAY, -2, @Now)
    );

BEGIN TRY
    BEGIN TRANSACTION;

    INSERT INTO Cases (
        MerchantId,
        CreatedByUserId,
        Title,
        Description,
        OfficialUrl,
        City,
        Address,
        WantedKolCount,
        ApplicationDeadline,
        SubmissionDeadline,
        CashRewardAmount,
        IsCommissionEnabled,
        CommissionRate,
        CookieDays,
        DeliverableDescription,
        Status,
        RecruitmentStatus,
        AutoExecutionThresholdRate,
        AutoExecutionThresholdCount,
        ApplicationCount,
        ApprovedAssignmentCount,
        PublishedAt,
        StartedAt,
        CompletedAt,
        SettledAt,
        CancelledAt
    )
    SELECT
        @MerchantId,
        @CreatedByUserId,
        s.Title,
        s.Description,
        N'https://example.com/case-seed',
        s.City,
        s.Address,
        s.WantedKolCount,
        s.ApplicationDeadline,
        s.SubmissionDeadline,
        s.CashRewardAmount,
        0,
        NULL,
        NULL,
        N'Instagram 圖文貼文 1 篇，至少 6 張照片；限時動態 3 則，並標註指定帳號與活動 Hashtag。',
        s.Status,
        s.RecruitmentStatus,
        50.00,
        CEILING(s.WantedKolCount * 0.5),
        0,
        0,
        s.PublishedAt,
        s.StartedAt,
        s.CompletedAt,
        s.SettledAt,
        s.CancelledAt
    FROM @CaseSeeds AS s
    WHERE NOT EXISTS (
        SELECT 1
        FROM Cases AS c
        WHERE c.MerchantId = @MerchantId
          AND c.Title = s.Title
    );

    INSERT INTO CasePlatforms (CaseId, Platform)
    SELECT c.Id, p.Platform
    FROM Cases AS c
    INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
    CROSS JOIN (VALUES (1), (5)) AS p(Platform)
    WHERE c.MerchantId = @MerchantId
      AND NOT EXISTS (
          SELECT 1
          FROM CasePlatforms AS existing
          WHERE existing.CaseId = c.Id
            AND existing.Platform = p.Platform
      );

    INSERT INTO CaseCategories (CaseId, Category)
    SELECT c.Id, 2
    FROM Cases AS c
    INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
    WHERE c.MerchantId = @MerchantId
      AND NOT EXISTS (
          SELECT 1 FROM CaseCategories AS existing WHERE existing.CaseId = c.Id
      );

    INSERT INTO CaseLanguages (CaseId, Language)
    SELECT c.Id, 1
    FROM Cases AS c
    INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
    WHERE c.MerchantId = @MerchantId
      AND NOT EXISTS (
          SELECT 1 FROM CaseLanguages AS existing WHERE existing.CaseId = c.Id
      );

    INSERT INTO CaseRequirements (CaseId, MinFollowers, Notes)
    SELECT
        c.Id,
        10000,
        N'旅遊或生活風格內容優先；條件僅供業者篩選參考，不阻擋 KOL 報名。'
    FROM Cases AS c
    INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
    WHERE c.MerchantId = @MerchantId
      AND NOT EXISTS (
          SELECT 1 FROM CaseRequirements AS existing WHERE existing.CaseId = c.Id
      );

    INSERT INTO CaseBarterItems (CaseId, Name, Quantity, Note)
    SELECT
        c.Id,
        N'雙人房住宿一晚（含早餐）',
        1,
        N'體驗日期需於錄取後與業者確認。'
    FROM Cases AS c
    INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
    WHERE c.MerchantId = @MerchantId
      AND NOT EXISTS (
          SELECT 1 FROM CaseBarterItems AS existing WHERE existing.CaseId = c.Id
      );

    INSERT INTO CaseBudgetSnapshots (
        CaseId,
        RewardAmountPerKol,
        WantedKolCount,
        RewardSubtotal,
        FeeItems,
        EstimatedFrozenAmount,
        SettingsSnapshot
    )
    SELECT
        c.Id,
        s.CashRewardAmount,
        s.WantedKolCount,
        s.CashRewardAmount * s.WantedKolCount,
        N'[{"code":"kol_service_fee","rate":0,"amount":0},{"code":"case_opening_fee","amount":1000}]',
        1000.00,
        N'{"kol_service_fee_rate":0,"case_opening_fee_amount":1000,"case_auto_execution_threshold_rate":50}'
    FROM Cases AS c
    INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
    WHERE c.MerchantId = @MerchantId
      AND NOT EXISTS (
          SELECT 1 FROM CaseBudgetSnapshots AS existing WHERE existing.CaseId = c.Id
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    c.Id,
    c.Title,
    c.Status,
    CASE c.Status
        WHEN 1 THEN 'Draft'
        WHEN 2 THEN 'Recruiting'
        WHEN 3 THEN 'RecruitmentClosed'
        WHEN 4 THEN 'InProgress'
        WHEN 5 THEN 'Completed'
        WHEN 6 THEN 'Settled'
        WHEN 7 THEN 'Cancelled'
    END AS StatusName,
    c.RecruitmentStatus,
    c.ApplicationDeadline,
    c.SubmissionDeadline
FROM Cases AS c
INNER JOIN @CaseSeeds AS s ON s.Title = c.Title
WHERE c.MerchantId = @MerchantId
ORDER BY c.Status;
