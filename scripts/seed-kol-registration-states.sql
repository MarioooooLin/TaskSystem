-- ================================================================
-- Seed: KOL registration flow states
-- Purpose: one fake KOL for each registration/review scenario.
-- Safe to run multiple times. Existing rows are reused by email.
-- ================================================================

DECLARE @AdminUserId BIGINT;

SELECT TOP 1 @AdminUserId = Id
FROM Users
WHERE AccountType = 1
ORDER BY Id;

IF @AdminUserId IS NULL
BEGIN
    SELECT TOP 1 @AdminUserId = Id
    FROM Users
    ORDER BY Id;
END;

-- 1. Fresh account only: user exists, profile not completed yet.
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'fresh-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Fresh KOL 01', N'fresh-kol-01@example.com', NULL, 1);
END;

-- 2. Basic profile draft: profile exists, but no review event yet.
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'draft-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Draft KOL 01', N'draft-kol-01@example.com', NULL, 1);
END;

DECLARE @DraftUserId BIGINT;
SELECT @DraftUserId = Id FROM Users WHERE Email = N'draft-kol-01@example.com';

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @DraftUserId)
BEGIN
    INSERT INTO KolProfiles (
        UserId, DisplayName, RealName, Phone, LineContactId, Intro,
        AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount,
        VerificationStatus, CreatedAt, UpdatedAt
    )
    VALUES (
        @DraftUserId, N'Draft Traveler Amy', N'Amy Lin', N'0912-000-102', N'@draftamy',
        N'Profile started but social accounts and payment info are not completed yet.',
        1, 1, 0, 0, 1, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE()
    );
END;

-- Users for review scenarios.
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'pending-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Pending KOL 01', N'pending-kol-01@example.com', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'social-check-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Social Check KOL 01', N'social-check-kol-01@example.com', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'bank-pending-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Bank Pending KOL 01', N'bank-pending-kol-01@example.com', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'returned-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Returned KOL 01', N'returned-kol-01@example.com', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'resubmit-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Resubmit KOL 01', N'resubmit-kol-01@example.com', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'approved-kol-01@example.com')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (3, N'Approved KOL 01', N'approved-kol-01@example.com', NULL, 1);
END;

DECLARE
    @PendingUserId BIGINT,
    @SocialCheckUserId BIGINT,
    @BankPendingUserId BIGINT,
    @ReturnedUserId BIGINT,
    @ResubmitUserId BIGINT,
    @ApprovedUserId BIGINT;

SELECT @PendingUserId = Id FROM Users WHERE Email = N'pending-kol-01@example.com';
SELECT @SocialCheckUserId = Id FROM Users WHERE Email = N'social-check-kol-01@example.com';
SELECT @BankPendingUserId = Id FROM Users WHERE Email = N'bank-pending-kol-01@example.com';
SELECT @ReturnedUserId = Id FROM Users WHERE Email = N'returned-kol-01@example.com';
SELECT @ResubmitUserId = Id FROM Users WHERE Email = N'resubmit-kol-01@example.com';
SELECT @ApprovedUserId = Id FROM Users WHERE Email = N'approved-kol-01@example.com';

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @PendingUserId)
BEGIN
    INSERT INTO KolProfiles (UserId, DisplayName, RealName, Phone, LineContactId, Intro, AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@PendingUserId, N'Taipei Weekend Sean', N'Sean Chen', N'0912-000-201', N'@seanweekend', N'Taipei cafes, alleys, and short trip stories.', 1, 1, 1, 18500, 1, DATEADD(day, -3, GETUTCDATE()), DATEADD(hour, -4, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @SocialCheckUserId)
BEGIN
    INSERT INTO KolProfiles (UserId, DisplayName, RealName, Phone, LineContactId, Intro, AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@SocialCheckUserId, N'Island Photo Nora', N'Nora Chang', N'0912-000-202', N'@nora_island', N'Beach stays, travel photography, and hotel reels.', 1, 1, 0, 126000, 1, DATEADD(day, -2, GETUTCDATE()), DATEADD(hour, -2, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @BankPendingUserId)
BEGIN
    INSERT INTO KolProfiles (UserId, DisplayName, RealName, Phone, LineContactId, Intro, AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@BankPendingUserId, N'Family Stay Mina', N'Mina Huang', N'0912-000-203', N'@minafamily', N'Family hotels, kid-friendly trips, and staycation notes.', 1, 1, 0, 42000, 1, DATEADD(day, -1, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @ReturnedUserId)
BEGIN
    INSERT INTO KolProfiles (UserId, DisplayName, RealName, Phone, LineContactId, Intro, AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount, VerificationStatus, RejectionNote, CreatedAt, UpdatedAt)
    VALUES (@ReturnedUserId, N'Food Trip Kai', N'Kai Lee', N'0912-000-204', N'@kaifoodtrip', N'Local food and slow travel stories.', 1, 1, 1, 9600, 3, N'Please provide verifiable social profile links and follower screenshots.', DATEADD(day, -6, GETUTCDATE()), DATEADD(day, -1, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @ResubmitUserId)
BEGIN
    INSERT INTO KolProfiles (UserId, DisplayName, RealName, Phone, LineContactId, Intro, AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@ResubmitUserId, N'Outdoor Camp Hana', N'Hana Wu', N'0912-000-205', N'@hana_camp', N'Hiking, camping, outdoor stays, and gear notes.', 1, 1, 0, 33500, 1, DATEADD(day, -8, GETUTCDATE()), DATEADD(hour, -3, GETUTCDATE()));
END;

IF NOT EXISTS (SELECT 1 FROM KolProfiles WHERE UserId = @ApprovedUserId)
BEGIN
    INSERT INTO KolProfiles (UserId, DisplayName, RealName, Phone, LineContactId, Intro, AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount, VerificationStatus, VerifiedAt, VerifiedByAdminId, CreatedAt, UpdatedAt)
    VALUES (@ApprovedUserId, N'City Stay Leo', N'Leo Wang', N'0912-000-206', N'@leocitystay', N'City hotel reviews, short videos, and business trip content.', 1, 1, 1, 87500, 2, DATEADD(hour, -6, GETUTCDATE()), @AdminUserId, DATEADD(day, -10, GETUTCDATE()), DATEADD(hour, -6, GETUTCDATE()));
END;

DECLARE
    @PendingKolId BIGINT,
    @SocialCheckKolId BIGINT,
    @BankPendingKolId BIGINT,
    @ReturnedKolId BIGINT,
    @ResubmitKolId BIGINT,
    @ApprovedKolId BIGINT;

SELECT @PendingKolId = Id FROM KolProfiles WHERE UserId = @PendingUserId;
SELECT @SocialCheckKolId = Id FROM KolProfiles WHERE UserId = @SocialCheckUserId;
SELECT @BankPendingKolId = Id FROM KolProfiles WHERE UserId = @BankPendingUserId;
SELECT @ReturnedKolId = Id FROM KolProfiles WHERE UserId = @ReturnedUserId;
SELECT @ResubmitKolId = Id FROM KolProfiles WHERE UserId = @ResubmitUserId;
SELECT @ApprovedKolId = Id FROM KolProfiles WHERE UserId = @ApprovedUserId;

-- Categories, areas, languages.
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @PendingKolId AND Category = 2) INSERT INTO KolCategories (KolId, Category) VALUES (@PendingKolId, 2);
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @SocialCheckKolId AND Category = 2) INSERT INTO KolCategories (KolId, Category) VALUES (@SocialCheckKolId, 2);
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @SocialCheckKolId AND Category = 11) INSERT INTO KolCategories (KolId, Category) VALUES (@SocialCheckKolId, 11);
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @BankPendingKolId AND Category = 6) INSERT INTO KolCategories (KolId, Category) VALUES (@BankPendingKolId, 6);
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @ReturnedKolId AND Category = 20) INSERT INTO KolCategories (KolId, Category) VALUES (@ReturnedKolId, 20);
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @ResubmitKolId AND Category = 19) INSERT INTO KolCategories (KolId, Category) VALUES (@ResubmitKolId, 19);
IF NOT EXISTS (SELECT 1 FROM KolCategories WHERE KolId = @ApprovedKolId AND Category = 2) INSERT INTO KolCategories (KolId, Category) VALUES (@ApprovedKolId, 2);

IF NOT EXISTS (SELECT 1 FROM KolServiceAreas WHERE KolId = @PendingKolId AND AreaCode = N'Taipei') INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName) VALUES (@PendingKolId, N'Taipei', N'Taipei');
IF NOT EXISTS (SELECT 1 FROM KolServiceAreas WHERE KolId = @SocialCheckKolId AND AreaCode = N'Kenting') INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName) VALUES (@SocialCheckKolId, N'Kenting', N'Kenting');
IF NOT EXISTS (SELECT 1 FROM KolServiceAreas WHERE KolId = @BankPendingKolId AND AreaCode = N'Taichung') INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName) VALUES (@BankPendingKolId, N'Taichung', N'Taichung');
IF NOT EXISTS (SELECT 1 FROM KolServiceAreas WHERE KolId = @ReturnedKolId AND AreaCode = N'Tainan') INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName) VALUES (@ReturnedKolId, N'Tainan', N'Tainan');
IF NOT EXISTS (SELECT 1 FROM KolServiceAreas WHERE KolId = @ResubmitKolId AND AreaCode = N'Yilan') INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName) VALUES (@ResubmitKolId, N'Yilan', N'Yilan');
IF NOT EXISTS (SELECT 1 FROM KolServiceAreas WHERE KolId = @ApprovedKolId AND AreaCode = N'Taipei') INSERT INTO KolServiceAreas (KolId, AreaCode, AreaName) VALUES (@ApprovedKolId, N'Taipei', N'Taipei');

IF NOT EXISTS (SELECT 1 FROM KolLanguages WHERE KolId = @PendingKolId AND LanguageCode = N'zh-TW') INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName) VALUES (@PendingKolId, N'zh-TW', N'Traditional Chinese');
IF NOT EXISTS (SELECT 1 FROM KolLanguages WHERE KolId = @SocialCheckKolId AND LanguageCode = N'zh-TW') INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName) VALUES (@SocialCheckKolId, N'zh-TW', N'Traditional Chinese');
IF NOT EXISTS (SELECT 1 FROM KolLanguages WHERE KolId = @BankPendingKolId AND LanguageCode = N'zh-TW') INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName) VALUES (@BankPendingKolId, N'zh-TW', N'Traditional Chinese');
IF NOT EXISTS (SELECT 1 FROM KolLanguages WHERE KolId = @ReturnedKolId AND LanguageCode = N'zh-TW') INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName) VALUES (@ReturnedKolId, N'zh-TW', N'Traditional Chinese');
IF NOT EXISTS (SELECT 1 FROM KolLanguages WHERE KolId = @ResubmitKolId AND LanguageCode = N'zh-TW') INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName) VALUES (@ResubmitKolId, N'zh-TW', N'Traditional Chinese');
IF NOT EXISTS (SELECT 1 FROM KolLanguages WHERE KolId = @ApprovedKolId AND LanguageCode = N'en') INSERT INTO KolLanguages (KolId, LanguageCode, LanguageName) VALUES (@ApprovedKolId, N'en', N'English');

-- Social accounts: Platform 2=Instagram, 4=YouTube, 5=Blog, 7=TikTok.
IF NOT EXISTS (SELECT 1 FROM KolSocialAccounts WHERE KolId = @PendingKolId AND Platform = 2)
    INSERT INTO KolSocialAccounts (KolId, Platform, AccountName, ProfileUrl, FollowersCount, DataSource, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@PendingKolId, 2, N'@sean.weekend', N'https://instagram.com/sean.weekend', 18500, 2, 2, DATEADD(day, -3, GETUTCDATE()), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM KolSocialAccounts WHERE KolId = @SocialCheckKolId AND Platform = 2)
    INSERT INTO KolSocialAccounts (KolId, Platform, AccountName, ProfileUrl, FollowersCount, DataSource, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@SocialCheckKolId, 2, N'@nora.island', N'https://instagram.com/nora.island', 126000, 2, 3, DATEADD(day, -2, GETUTCDATE()), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM KolSocialAccounts WHERE KolId = @BankPendingKolId AND Platform = 2)
    INSERT INTO KolSocialAccounts (KolId, Platform, AccountName, ProfileUrl, FollowersCount, DataSource, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@BankPendingKolId, 2, N'@mina.familytrip', N'https://instagram.com/mina.familytrip', 42000, 2, 2, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM KolSocialAccounts WHERE KolId = @ReturnedKolId AND Platform = 5)
    INSERT INTO KolSocialAccounts (KolId, Platform, AccountName, ProfileUrl, FollowersCount, DataSource, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@ReturnedKolId, 5, N'kaifoodtrip.blog', N'https://kaifoodtrip.example.com', 9600, 2, 3, DATEADD(day, -6, GETUTCDATE()), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM KolSocialAccounts WHERE KolId = @ResubmitKolId AND Platform = 7)
    INSERT INTO KolSocialAccounts (KolId, Platform, AccountName, ProfileUrl, FollowersCount, DataSource, VerificationStatus, CreatedAt, UpdatedAt)
    VALUES (@ResubmitKolId, 7, N'@hana.camp', N'https://www.tiktok.com/@hana.camp', 33500, 2, 2, DATEADD(day, -8, GETUTCDATE()), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM KolSocialAccounts WHERE KolId = @ApprovedKolId AND Platform = 4)
    INSERT INTO KolSocialAccounts (KolId, Platform, AccountName, ProfileUrl, FollowersCount, DataSource, VerificationStatus, LastSyncAt, CreatedAt, UpdatedAt)
    VALUES (@ApprovedKolId, 4, N'Leo City Stay', N'https://youtube.com/@leocitystay', 87500, 1, 1, DATEADD(hour, -8, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()), GETUTCDATE());

-- Bank accounts: only specific scenarios need bank status.
IF NOT EXISTS (SELECT 1 FROM KolBankAccounts WHERE KolId = @BankPendingKolId)
BEGIN
    INSERT INTO KolBankAccounts (KolId, AccountType, AccountName, BankCode, BankName, BranchCode, BranchName, AccountNumberEncrypted, Status, CreatedAt, UpdatedAt)
    VALUES (@BankPendingKolId, 1, N'Mina Huang', N'822', N'CTBC Bank', N'0012', N'Taichung Branch', N'seed-encrypted-822-000203', 1, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM KolBankAccounts WHERE KolId = @ApprovedKolId)
BEGIN
    INSERT INTO KolBankAccounts (KolId, AccountType, AccountName, BankCode, BankName, BranchCode, BranchName, AccountNumberEncrypted, Status, CreatedAt, UpdatedAt)
    VALUES (@ApprovedKolId, 1, N'Leo Wang', N'013', N'Cathay United Bank', N'0045', N'Xinyi Branch', N'seed-encrypted-013-000206', 2, DATEADD(day, -10, GETUTCDATE()), DATEADD(hour, -6, GETUTCDATE()));
END;

-- Review events: 1=Submitted, 2=Resubmitted, 3=Approved, 4=Returned.
IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @PendingKolId AND ActionType = 1)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@PendingKolId, 1, NULL, 1, N'First submission.', @PendingUserId, DATEADD(hour, -4, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @SocialCheckKolId AND ActionType = 1)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@SocialCheckKolId, 1, NULL, 1, N'First submission. Social follower count requires manual confirmation.', @SocialCheckUserId, DATEADD(hour, -2, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @BankPendingKolId AND ActionType = 1)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@BankPendingKolId, 1, NULL, 1, N'First submission. Payment information is pending confirmation.', @BankPendingUserId, DATEADD(hour, -1, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ReturnedKolId AND ActionType = 1)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ReturnedKolId, 1, NULL, 1, N'First submission.', @ReturnedUserId, DATEADD(day, -5, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ReturnedKolId AND ActionType = 4)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ReturnedKolId, 4, 1, 3, N'Please provide verifiable social profile links and follower screenshots.', @AdminUserId, DATEADD(day, -1, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ResubmitKolId AND ActionType = 1)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ResubmitKolId, 1, NULL, 1, N'First submission.', @ResubmitUserId, DATEADD(day, -7, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ResubmitKolId AND ActionType = 4)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ResubmitKolId, 4, 1, 3, N'Please add outdoor collaboration references.', @AdminUserId, DATEADD(day, -4, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ResubmitKolId AND ActionType = 2)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ResubmitKolId, 2, 3, 1, N'Collaboration references added. Resubmitted for review.', @ResubmitUserId, DATEADD(hour, -3, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ApprovedKolId AND ActionType = 1)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ApprovedKolId, 1, NULL, 1, N'First submission.', @ApprovedUserId, DATEADD(day, -9, GETUTCDATE()));

IF NOT EXISTS (SELECT 1 FROM KolReviewEvents WHERE KolId = @ApprovedKolId AND ActionType = 3)
    INSERT INTO KolReviewEvents (KolId, ActionType, FromStatus, ToStatus, Comment, ActorUserId, CreatedAt)
    VALUES (@ApprovedKolId, 3, 1, 2, N'Profile completed. Approved.', @AdminUserId, DATEADD(hour, -6, GETUTCDATE()));

SELECT N'Seed completed: 8 KOL registration/review scenarios.' AS Result;
