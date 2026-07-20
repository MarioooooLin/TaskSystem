-- ================================================================
-- SEED DATA - demo rows for current admin features
-- Target: SQL Server. Safe to run repeatedly by [SEED15] keys.
-- Covers: merchants, KOLs, review states, cases, applications,
-- tasks, submissions, disputes, finance wallet/credit rows, logs.
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @Now DATETIME2 = GETUTCDATE();
DECLARE @AdminUserId BIGINT;
SELECT TOP (1) @AdminUserId = Id
FROM Users
WHERE AccountType = 1
ORDER BY Id;
IF @AdminUserId IS NULL BEGIN
INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
VALUES (
        1,
        N'[SEED15] Admin',
        N'seed15-admin@example.com',
        NULL,
        1
    );
SELECT @AdminUserId = SCOPE_IDENTITY();
END;
BEGIN TRY BEGIN TRANSACTION;
DECLARE @i INT;
-- ------------------------------------------------------------
-- 1. Merchants: 15 owners + 15 merchants + wallets + contacts
-- ------------------------------------------------------------
SET @i = 1;
WHILE @i <= 15 BEGIN
DECLARE @MerchantEmail NVARCHAR(256) = CONCAT(
        N'seed15-merchant-',
        FORMAT(@i, '00'),
        N'@example.com'
    );
DECLARE @MerchantUserId BIGINT;
DECLARE @MerchantId BIGINT;
IF NOT EXISTS (
    SELECT 1
    FROM Users
    WHERE Email = @MerchantEmail
) BEGIN
INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
VALUES (
        2,
        CONCAT(N'[SEED15] 業者 Owner ', FORMAT(@i, '00')),
        @MerchantEmail,
        NULL,
        CASE
            WHEN @i % 13 = 0 THEN 2
            ELSE 1
        END
    );
END;
SELECT @MerchantUserId = Id
FROM Users
WHERE Email = @MerchantEmail;
IF NOT EXISTS (
    SELECT 1
    FROM Merchants
    WHERE UserId = @MerchantUserId
) BEGIN
INSERT INTO Merchants (
        UserId,
        CompanyName,
        EnglishName,
        TaxId,
        IndustryType,
        ContactName,
        Phone,
        Fax,
        CompanyEmail,
        Website,
        Address,
        EstablishedDate,
        VerificationStatus,
        VerifiedAt,
        UpdatedByAdminId,
        CreatedAt,
        UpdatedAt
    )
VALUES (
        @MerchantUserId,
        CONCAT(N'[SEED15] 旅宿品牌 ', FORMAT(@i, '00')),
        CONCAT(N'Seed Stay ', FORMAT(@i, '00')),
        CONCAT(N'88', FORMAT(@i, '000000')),
        CASE
            @i % 4
            WHEN 0 THEN N'餐飲'
            WHEN 1 THEN N'旅宿'
            WHEN 2 THEN N'體驗活動'
            ELSE N'交通票券'
        END,
        CONCAT(N'業者窗口 ', FORMAT(@i, '00')),
        CONCAT(N'02-2700-', FORMAT(@i, '0000')),
        NULL,
        CONCAT(
            N'service',
            FORMAT(@i, '00'),
            N'@seedstay.example.com'
        ),
        CONCAT(
            N'https://seedstay.example.com/',
            FORMAT(@i, '00')
        ),
        CONCAT(N'台灣測試市測試區 ', @i, N' 號'),
        DATEADD(YEAR, -(@i + 3), CAST(@Now AS DATE)),
        CASE
            WHEN @i IN (4, 9) THEN 4
            ELSE 2
        END,
        CASE
            WHEN @i IN (4, 9) THEN NULL
            ELSE DATEADD(DAY, - @i, @Now)
        END,
        @AdminUserId,
        DATEADD(DAY, - @i * 2, @Now),
        DATEADD(DAY, - @i, @Now)
    );
END;
SELECT @MerchantId = Id
FROM Merchants
WHERE UserId = @MerchantUserId;
IF NOT EXISTS (
    SELECT 1
    FROM MerchantWallets
    WHERE MerchantId = @MerchantId
)
INSERT INTO MerchantWallets (
        MerchantId,
        AvailableAmount,
        FrozenAmount,
        TotalDepositedAmount
    )
VALUES (
        @MerchantId,
        20000 + @i * 1500,
        @i * 800,
        80000 + @i * 10000
    );
IF NOT EXISTS (
    SELECT 1
    FROM MerchantCreditWallets
    WHERE MerchantId = @MerchantId
)
INSERT INTO MerchantCreditWallets (
        MerchantId,
        AvailableAmount,
        UsedAmount,
        ExpiredAmount,
        RevokedAmount,
        UpdatedAt
    )
VALUES (
        @MerchantId,
        3000 + @i * 200,
        @i * 100,
        0,
        0,
        @Now
    );
IF NOT EXISTS (
    SELECT 1
    FROM MerchantContacts
    WHERE MerchantId = @MerchantId
        AND Email = CONCAT(
            N'contact',
            FORMAT(@i, '00'),
            N'@seedstay.example.com'
        )
)
INSERT INTO MerchantContacts (MerchantId, Name, Phone, Email, Title, Note)
VALUES (
        @MerchantId,
        CONCAT(N'合作窗口 ', FORMAT(@i, '00')),
        CONCAT(N'09', FORMAT(12000000 + @i, '00000000')),
        CONCAT(
            N'contact',
            FORMAT(@i, '00'),
            N'@seedstay.example.com'
        ),
        N'行銷企劃',
        N'[SEED15] 測試聯絡人'
    );
SET @i + = 1;
END;
-- ------------------------------------------------------------
-- 2. KOLs: 15 profiles + social/category/bank/review/wallet
-- ------------------------------------------------------------
SET @i = 1;
WHILE @i <= 15 BEGIN
DECLARE @KolEmail NVARCHAR(256) = CONCAT(
        N'seed15-kol-',
        FORMAT(@i, '00'),
        N'@example.com'
    );
DECLARE @KolUserId BIGINT;
DECLARE @KolId BIGINT;
DECLARE @KolStatus SMALLINT = CASE
        WHEN @i IN (3, 8, 13) THEN 1
        WHEN @i IN (5, 10) THEN 3
        WHEN @i = 15 THEN 4
        ELSE 2
    END;
IF NOT EXISTS (
    SELECT 1
    FROM Users
    WHERE Email = @KolEmail
) BEGIN
INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
VALUES (
        3,
        CONCAT(N'[SEED15] KOL User ', FORMAT(@i, '00')),
        @KolEmail,
        NULL,
        CASE
            WHEN @i = 15 THEN 2
            ELSE 1
        END
    );
END;
SELECT @KolUserId = Id
FROM Users
WHERE Email = @KolEmail;
IF NOT EXISTS (
    SELECT 1
    FROM KolProfiles
    WHERE UserId = @KolUserId
) BEGIN
INSERT INTO KolProfiles (
        UserId,
        DisplayName,
        RealName,
        Phone,
        LineContactId,
        Intro,
        AcceptsCash,
        AcceptsBarter,
        AcceptsCommission,
        FollowersCount,
        VerificationStatus,
        VerifiedAt,
        VerifiedByAdminId,
        RejectionNote,
        SuspensionNote,
        CreatedAt,
        UpdatedAt
    )
VALUES (
        @KolUserId,
        CONCAT(N'[SEED15] 旅行創作者 ', FORMAT(@i, '00')),
        CONCAT(N'測試創作者 ', FORMAT(@i, '00')),
        CONCAT(
            N'0912-',
            FORMAT(@i, '000'),
            N'-',
            FORMAT(@i * 7, '000')
        ),
        CONCAT(N'@seedkol', FORMAT(@i, '00')),
        CONCAT(N'[SEED15] 旅遊、住宿、生活風格內容測試資料 ', FORMAT(@i, '00')),
        1,
        CASE
            WHEN @i % 2 = 0 THEN 1
            ELSE 0
        END,
        CASE
            WHEN @i % 3 = 0 THEN 1
            ELSE 0
        END,
        5000 + @i * 7500,
        @KolStatus,
        CASE
            WHEN @KolStatus = 2 THEN DATEADD(DAY, - @i, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @KolStatus = 2 THEN @AdminUserId
            ELSE NULL
        END,
        CASE
            WHEN @KolStatus = 3 THEN N'[SEED15] 社群連結或截圖需補件。'
            ELSE NULL
        END,
        CASE
            WHEN @KolStatus = 4 THEN N'[SEED15] 測試停權狀態。'
            ELSE NULL
        END,
        DATEADD(DAY, - @i * 3, @Now),
        DATEADD(DAY, - @i, @Now)
    );
END;
SELECT @KolId = Id
FROM KolProfiles
WHERE UserId = @KolUserId;
IF NOT EXISTS (
    SELECT 1
    FROM KolCategories
    WHERE KolId = @KolId
)
INSERT INTO KolCategories (KolId, Category)
VALUES (
        @KolId,
        CASE
            WHEN @i % 5 = 0 THEN 20
            WHEN @i % 3 = 0 THEN 11
            ELSE 2
        END
    );
IF OBJECT_ID(N'KolServiceAreas', N'U') IS NOT NULL IF NOT EXISTS (
    SELECT 1
    FROM KolServiceAreas
    WHERE KolId = @KolId
)
INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName)
VALUES (
        @KolId,
        CASE
            WHEN @i % 2 = 0 THEN N'Taipei'
            ELSE N'Tainan'
        END,
        CASE
            WHEN @i % 2 = 0 THEN N'台北'
            ELSE N'台南'
        END
    );
IF OBJECT_ID(N'KolLanguages', N'U') IS NOT NULL IF NOT EXISTS (
    SELECT 1
    FROM KolLanguages
    WHERE KolId = @KolId
)
INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName)
VALUES (@KolId, N'zh-TW', N'繁體中文');
IF NOT EXISTS (
    SELECT 1
    FROM KolSocialAccounts
    WHERE KolId = @KolId
)
INSERT INTO KolSocialAccounts (
        KolId,
        Platform,
        AccountName,
        ProfileUrl,
        FollowersCount,
        DataSource,
        VerificationStatus,
        LastSyncAt,
        CreatedAt,
        UpdatedAt
    )
VALUES (
        @KolId,
        CASE
            WHEN @i % 4 = 0 THEN 4
            WHEN @i % 4 = 1 THEN 2
            WHEN @i % 4 = 2 THEN 7
            ELSE 5
        END,
        CONCAT(N'@seedkol', FORMAT(@i, '00')),
        CONCAT(
            N'https://social.example.com/seedkol',
            FORMAT(@i, '00')
        ),
        5000 + @i * 7500,
        2,
        CASE
            WHEN @KolStatus = 3 THEN 3
            ELSE 2
        END,
        DATEADD(HOUR, - @i, @Now),
        DATEADD(DAY, - @i * 3, @Now),
        @Now
    );
IF NOT EXISTS (
    SELECT 1
    FROM KolBankAccounts
    WHERE KolId = @KolId
)
INSERT INTO KolBankAccounts (
        KolId,
        AccountType,
        AccountName,
        BankCode,
        BankName,
        BranchCode,
        BranchName,
        AccountNumberEncrypted,
        Status,
        CreatedAt,
        UpdatedAt
    )
VALUES (
        @KolId,
        1,
        CONCAT(N'測試創作者 ', FORMAT(@i, '00')),
        N'822',
        N'中國信託',
        N'001',
        N'測試分行',
        CONCAT(N'seed15-encrypted-', FORMAT(@i, '00')),
        CASE
            WHEN @i % 5 = 0 THEN 1
            ELSE 2
        END,
        DATEADD(DAY, - @i, @Now),
        @Now
    );
IF OBJECT_ID(N'KolWallets', N'U') IS NOT NULL IF NOT EXISTS (
    SELECT 1
    FROM KolWallets
    WHERE KolId = @KolId
)
INSERT INTO KolWallets (
        KolId,
        PendingAmount,
        AvailableAmount,
        PaidAmount,
        UpdatedAt
    )
VALUES (@KolId, @i * 500, @i * 1200, @i * 800, @Now);
IF OBJECT_ID(N'KolReviewEvents', N'U') IS NOT NULL IF NOT EXISTS (
    SELECT 1
    FROM KolReviewEvents
    WHERE KolId = @KolId
)
INSERT INTO KolReviewEvents (
        KolId,
        ActionType,
        FromStatus,
        ToStatus,
        Comment,
        ActorUserId,
        CreatedAt
    )
VALUES (
        @KolId,
        CASE
            WHEN @KolStatus = 2 THEN 3
            WHEN @KolStatus = 3 THEN 4
            ELSE 1
        END,
        NULL,
        @KolStatus,
        N'[SEED15] KOL 審核流程測試資料',
        CASE
            WHEN @KolStatus IN (2, 3, 4) THEN @AdminUserId
            ELSE @KolUserId
        END,
        DATEADD(DAY, - @i, @Now)
    );
SET @i + = 1;
END;
-- ------------------------------------------------------------
-- 3. Cases: 15 cases + case metadata
-- ------------------------------------------------------------
SET @i = 1;
WHILE @i <= 15 BEGIN
DECLARE @CaseMerchantId BIGINT;
DECLARE @CaseOwnerId BIGINT;
DECLARE @CaseId BIGINT;
DECLARE @CaseTitle NVARCHAR(200) = CONCAT(N'[SEED15] 旅遊合作案件 ', FORMAT(@i, '00'));
DECLARE @CaseStatus SMALLINT = ((@i - 1) % 7) + 1;
SELECT @CaseMerchantId = Id,
    @CaseOwnerId = UserId
FROM (
        SELECT m.Id,
            m.UserId,
            ROW_NUMBER() OVER (
                ORDER BY m.Id
            ) AS rn
        FROM Merchants m
            INNER JOIN Users u ON u.Id = m.UserId
        WHERE u.Email LIKE N'seed15-merchant-%@example.com'
    ) s
WHERE rn = ((@i - 1) % 15) + 1;
IF NOT EXISTS (
    SELECT 1
    FROM Cases
    WHERE MerchantId = @CaseMerchantId
        AND Title = @CaseTitle
) BEGIN
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
        CancelledAt,
        CreatedAt,
        UpdatedAt
    )
VALUES (
        @CaseMerchantId,
        @CaseOwnerId,
        @CaseTitle,
        CONCAT(
            N'[SEED15] 案件描述 ',
            FORMAT(@i, '00'),
            N'，用於列表、詳情、財務與案件監控測試。'
        ),
        CONCAT(
            N'https://cases.example.com/seed15/',
            FORMAT(@i, '00')
        ),
        CASE
            @i % 5
            WHEN 0 THEN N'台北市'
            WHEN 1 THEN N'台南市'
            WHEN 2 THEN N'花蓮縣'
            WHEN 3 THEN N'屏東縣'
            ELSE N'宜蘭縣'
        END,
        CONCAT(N'測試路 ', @i, N' 號'),
        2 + (@i % 4),
        CASE
            WHEN @CaseStatus = 1 THEN DATEADD(DAY, 30, @Now)
            ELSE DATEADD(DAY, 10 - @i, @Now)
        END,
        CASE
            WHEN @CaseStatus IN (5, 6) THEN DATEADD(DAY, -5, @Now)
            ELSE DATEADD(DAY, 25 - @i, @Now)
        END,
        3000 + @i * 600,
        CASE
            WHEN @i % 3 = 0 THEN 1
            ELSE 0
        END,
        CASE
            WHEN @i % 3 = 0 THEN 8.00
            ELSE NULL
        END,
        CASE
            WHEN @i % 3 = 0 THEN 30
            ELSE NULL
        END,
        N'Instagram 貼文 1 篇、短影音 1 支、限動 3 則。',
        @CaseStatus,
        CASE
            WHEN @CaseStatus = 1 THEN 1
            WHEN @CaseStatus = 2 THEN 2
            ELSE 3
        END,
        50.00,
        1,
        1,
        CASE
            WHEN @CaseStatus >= 3 THEN 1
            ELSE 0
        END,
        CASE
            WHEN @CaseStatus >= 2 THEN DATEADD(DAY, - @i, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @CaseStatus >= 4 THEN DATEADD(DAY, - @i + 1, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @CaseStatus >= 5
            AND @CaseStatus <> 7 THEN DATEADD(DAY, - @i + 2, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @CaseStatus = 6 THEN DATEADD(DAY, -1, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @CaseStatus = 7 THEN DATEADD(DAY, -1, @Now)
            ELSE NULL
        END,
        DATEADD(DAY, - @i * 2, @Now),
        DATEADD(HOUR, - @i, @Now)
    );
END;
SELECT @CaseId = Id
FROM Cases
WHERE MerchantId = @CaseMerchantId
    AND Title = @CaseTitle;
IF NOT EXISTS (
    SELECT 1
    FROM CasePlatforms
    WHERE CaseId = @CaseId
)
INSERT INTO CasePlatforms (CaseId, Platform)
VALUES (@CaseId, 2),
    (@CaseId, 9);
IF NOT EXISTS (
    SELECT 1
    FROM CaseCategories
    WHERE CaseId = @CaseId
)
INSERT INTO CaseCategories (CaseId, Category)
VALUES (
        @CaseId,
        CASE
            WHEN @i % 3 = 0 THEN 11
            ELSE 2
        END
    );
IF NOT EXISTS (
    SELECT 1
    FROM CaseLanguages
    WHERE CaseId = @CaseId
)
INSERT INTO CaseLanguages (CaseId, LanguageCode)
VALUES (@CaseId, N'zh-TW');
IF NOT EXISTS (
    SELECT 1
    FROM CaseRequirements
    WHERE CaseId = @CaseId
)
INSERT INTO CaseRequirements (CaseId, MinFollowers, Notes)
VALUES (@CaseId, 5000 + @i * 1000, N'[SEED15] 旅遊內容經驗優先。');
IF NOT EXISTS (
    SELECT 1
    FROM CaseBarterItems
    WHERE CaseId = @CaseId
)
INSERT INTO CaseBarterItems (CaseId, Name, Quantity, Note)
VALUES (@CaseId, N'雙人住宿一晚', 1, N'[SEED15] 體驗內容');
IF NOT EXISTS (
    SELECT 1
    FROM CaseBudgetSnapshots
    WHERE CaseId = @CaseId
)
INSERT INTO CaseBudgetSnapshots (
        CaseId,
        RewardAmountPerKol,
        WantedKolCount,
        RewardSubtotal,
        FeeItems,
        EstimatedFrozenAmount,
        SettingsSnapshot
    )
SELECT Id,
    CashRewardAmount,
    WantedKolCount,
    CashRewardAmount * WantedKolCount,
    N'[]',
    CashRewardAmount * WantedKolCount,
    N'{"seed":"seed15"}'
FROM Cases
WHERE Id = @CaseId;
SET @i + = 1;
END;
-- ------------------------------------------------------------
-- 4. Applications/tasks/submissions/disputes/finance/logs: 15
-- ------------------------------------------------------------
SET @i = 1;
WHILE @i <= 15 BEGIN
DECLARE @SeedCaseId BIGINT;
DECLARE @SeedMerchantId BIGINT;
DECLARE @SeedKolId BIGINT;
DECLARE @SeedTaskId BIGINT;
DECLARE @SeedApplicationId BIGINT;
DECLARE @SeedSubmissionId BIGINT;
DECLARE @TaskStatus SMALLINT = ((@i - 1) % 8) + 1;
SELECT @SeedCaseId = Id,
    @SeedMerchantId = MerchantId
FROM (
        SELECT Id,
            MerchantId,
            ROW_NUMBER() OVER (
                ORDER BY Id
            ) AS rn
        FROM Cases
        WHERE Title LIKE N'[[]SEED15] 旅遊合作案件%'
    ) c
WHERE rn = @i;
SELECT @SeedKolId = Id
FROM (
        SELECT kp.Id,
            ROW_NUMBER() OVER (
                ORDER BY kp.Id
            ) AS rn
        FROM KolProfiles kp
            INNER JOIN Users u ON u.Id = kp.UserId
        WHERE u.Email LIKE N'seed15-kol-%@example.com'
    ) k
WHERE rn = @i;
IF NOT EXISTS (
    SELECT 1
    FROM CaseApplications
    WHERE CaseId = @SeedCaseId
        AND KolId = @SeedKolId
)
INSERT INTO CaseApplications (
        CaseId,
        KolId,
        Status,
        Message,
        IsRequirementMatched,
        MismatchReasons,
        ReconfirmedAt,
        AppliedAt,
        ReviewedAt,
        ReviewedByUserId
    )
VALUES (
        @SeedCaseId,
        @SeedKolId,
        CASE
            WHEN @i % 6 = 0 THEN 4
            WHEN @i % 5 = 0 THEN 3
            ELSE 2
        END,
        N'[SEED15] 我想參與這個合作案件。',
        1,
        NULL,
        CASE
            WHEN @i % 5 = 0 THEN DATEADD(DAY, -1, @Now)
            ELSE NULL
        END,
        DATEADD(DAY, - @i, @Now),
        DATEADD(HOUR, - @i, @Now),
        @AdminUserId
    );
SELECT @SeedApplicationId = Id
FROM CaseApplications
WHERE CaseId = @SeedCaseId
    AND KolId = @SeedKolId;
IF NOT EXISTS (
    SELECT 1
    FROM Tasks
    WHERE CaseId = @SeedCaseId
        AND KolId = @SeedKolId
)
INSERT INTO Tasks (
        CaseId,
        KolId,
        ApplicationId,
        Status,
        CancellationSource,
        StartedAt,
        SubmittedAt,
        CompletedAt,
        CancelledAt
    )
VALUES (
        @SeedCaseId,
        @SeedKolId,
        @SeedApplicationId,
        @TaskStatus,
        CASE
            WHEN @TaskStatus = 8 THEN 2
            ELSE 0
        END,
        CASE
            WHEN @TaskStatus >= 3
            AND @TaskStatus <> 8 THEN DATEADD(DAY, - @i, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @TaskStatus IN (4, 5, 6, 7) THEN DATEADD(DAY, - @i + 1, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @TaskStatus = 6 THEN DATEADD(DAY, -1, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @TaskStatus = 8 THEN DATEADD(DAY, -1, @Now)
            ELSE NULL
        END
    );
SELECT @SeedTaskId = Id
FROM Tasks
WHERE CaseId = @SeedCaseId
    AND KolId = @SeedKolId;
IF NOT EXISTS (
    SELECT 1
    FROM Submissions
    WHERE TaskId = @SeedTaskId
) BEGIN
INSERT INTO Submissions (
        TaskId,
        KolId,
        Status,
        IsAutoApproved,
        Note,
        ReviewDeadlineAt,
        SubmittedAt,
        ReviewedAt,
        ReviewedByUserId
    )
VALUES (
        @SeedTaskId,
        @SeedKolId,
        CASE
            WHEN @TaskStatus = 6 THEN 3
            WHEN @TaskStatus = 5 THEN 2
            WHEN @TaskStatus = 7 THEN 4
            WHEN @TaskStatus = 8 THEN 5
            ELSE 1
        END,
        0,
        N'[SEED15] 成果提交測試資料',
        DATEADD(DAY, 14, @Now),
        DATEADD(DAY, - @i + 1, @Now),
        CASE
            WHEN @TaskStatus IN (5, 6, 7, 8) THEN DATEADD(HOUR, - @i, @Now)
            ELSE NULL
        END,
        CASE
            WHEN @TaskStatus IN (5, 6, 7, 8) THEN @AdminUserId
            ELSE NULL
        END
    );
SELECT @SeedSubmissionId = SCOPE_IDENTITY();
IF OBJECT_ID(N'SubmissionItems', N'U') IS NOT NULL
INSERT INTO SubmissionItems (SubmissionId, Url, Note)
VALUES (
        @SeedSubmissionId,
        CONCAT(
            N'https://posts.example.com/seed15/',
            FORMAT(@i, '00')
        ),
        N'[SEED15] 成果連結'
    );
END;
IF NOT EXISTS (
    SELECT 1
    FROM KolEarnings
    WHERE TaskId = @SeedTaskId
)
INSERT INTO KolEarnings (
        CaseId,
        TaskId,
        KolId,
        SourceType,
        GrossAmount,
        PlatformFeeAmount,
        NetAmount,
        Status,
        AvailableAt,
        CreatedAt
    )
VALUES (
        @SeedCaseId,
        @SeedTaskId,
        @SeedKolId,
        1,
        3000 + @i * 600,
        (3000 + @i * 600) * 0.10,
        (3000 + @i * 600) * 0.90,
        CASE
            WHEN @TaskStatus = 6 THEN 3
            WHEN @TaskStatus = 8 THEN 6
            ELSE 1
        END,
        CASE
            WHEN @TaskStatus = 6 THEN DATEADD(DAY, 7, @Now)
            ELSE NULL
        END,
        DATEADD(DAY, - @i, @Now)
    );
IF @i <= 15
AND NOT EXISTS (
    SELECT 1
    FROM MerchantWalletTransactions
    WHERE MerchantId = @SeedMerchantId
        AND RelatedCaseId = @SeedCaseId
        AND Note = N'[SEED15] 案件結算測試'
)
INSERT INTO MerchantWalletTransactions (
        MerchantId,
        Type,
        Amount,
        Status,
        RelatedCaseId,
        Note,
        CreatedAt
    )
VALUES (
        @SeedMerchantId,
        CASE
            WHEN @i % 4 = 0 THEN 4
            WHEN @i % 3 = 0 THEN 3
            ELSE 2
        END,
        1000 + @i * 700,
        2,
        @SeedCaseId,
        N'[SEED15] 案件結算測試',
        DATEADD(DAY, - @i, @Now)
    );
IF NOT EXISTS (
    SELECT 1
    FROM MerchantCreditTransactions
    WHERE MerchantId = @SeedMerchantId
        AND RelatedCaseId = @SeedCaseId
        AND Note = N'[SEED15] 折扣金測試'
)
INSERT INTO MerchantCreditTransactions (
        MerchantId,
        Type,
        Amount,
        Status,
        RelatedCaseId,
        Reason,
        Note,
        CreatedByUserId,
        CreatedAt
    )
VALUES (
        @SeedMerchantId,
        CASE
            WHEN @i % 2 = 0 THEN 2
            ELSE 1
        END,
        CASE
            WHEN @i % 2 = 0 THEN -(200 + @i * 50)
            ELSE 500 + @i * 100
        END,
        2,
        @SeedCaseId,
        N'[SEED15] 測試折扣金',
        N'[SEED15] 折扣金測試',
        @AdminUserId,
        DATEADD(DAY, - @i, @Now)
    );
IF NOT EXISTS (
    SELECT 1
    FROM Disputes
    WHERE TaskId = @SeedTaskId
)
INSERT INTO Disputes (
        CaseId,
        TaskId,
        OpenedByUserId,
        Reason,
        Description,
        Status,
        ResolutionNote,
        OpenedAt,
        ResolvedAt
    )
VALUES (
        @SeedCaseId,
        @SeedTaskId,
        @AdminUserId,
        CASE
            WHEN @i % 2 = 0 THEN N'成果驗收'
            ELSE N'合作履約'
        END,
        N'[SEED15] 爭議案件測試資料',
        CASE
            WHEN @i % 6 = 0 THEN 6
            WHEN @i % 5 = 0 THEN 5
            WHEN @i % 4 = 0 THEN 4
            WHEN @i % 3 = 0 THEN 3
            WHEN @i % 2 = 0 THEN 2
            ELSE 1
        END,
        CASE
            WHEN @i % 3 = 0 THEN N'[SEED15] 已完成測試處理。'
            ELSE NULL
        END,
        CASE
            WHEN @i = 1 THEN @Now
            ELSE DATEADD(DAY, - @i, @Now)
        END,
        CASE
            WHEN @i % 3 = 0 THEN DATEADD(HOUR, - @i, @Now)
            ELSE NULL
        END
    );
IF NOT EXISTS (
    SELECT 1
    FROM ActivityLogs
    WHERE TargetType = N'Case'
        AND TargetId = @SeedCaseId
        AND Action = N'[SEED15] Seeded'
)
INSERT INTO ActivityLogs (
        TargetType,
        TargetId,
        CaseId,
        ActorUserId,
        Action,
        Note,
        CreatedAt
    )
VALUES (
        N'Case',
        @SeedCaseId,
        @SeedCaseId,
        @AdminUserId,
        N'[SEED15] Seeded',
        N'建立測試案件、任務與財務資料',
        DATEADD(HOUR, - @i, @Now)
    );
SET @i + = 1;
END;
COMMIT TRANSACTION;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;
SELECT (
        SELECT COUNT(*)
        FROM Merchants m
            INNER JOIN Users u ON u.Id = m.UserId
        WHERE u.Email LIKE N'seed15-merchant-%@example.com'
    ) AS SeedMerchants,
    (
        SELECT COUNT(*)
        FROM KolProfiles kp
            INNER JOIN Users u ON u.Id = kp.UserId
        WHERE u.Email LIKE N'seed15-kol-%@example.com'
    ) AS SeedKols,
    (
        SELECT COUNT(*)
        FROM Cases
        WHERE Title LIKE N'[[]SEED15] 旅遊合作案件%'
    ) AS SeedCases,
    (
        SELECT COUNT(*)
        FROM CaseApplications ca
            INNER JOIN Cases c ON c.Id = ca.CaseId
        WHERE c.Title LIKE N'[[]SEED15] 旅遊合作案件%'
    ) AS SeedApplications,
    (
        SELECT COUNT(*)
        FROM Tasks t
            INNER JOIN Cases c ON c.Id = t.CaseId
        WHERE c.Title LIKE N'[[]SEED15] 旅遊合作案件%'
    ) AS SeedTasks,
    (
        SELECT COUNT(*)
        FROM Disputes d
            INNER JOIN Cases c ON c.Id = d.CaseId
        WHERE c.Title LIKE N'[[]SEED15] 旅遊合作案件%'
    ) AS SeedDisputes;