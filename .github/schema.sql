-- ================================================================
-- TaskSystem Database Schema
-- 資料庫：MSSQL (SQL Server)
-- 命名規則：PascalCase
-- 金額欄位：DECIMAL(12,2)
-- 狀態欄位：SMALLINT（對應 C# Enum，值見各欄位註解）
-- ================================================================
-- ================================================================
-- 1. 帳號與身份
-- ================================================================
CREATE TABLE Users (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    AccountType SMALLINT NOT NULL,
    -- 1=Admin  2=Merchant  3=Kol
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NULL,
    -- 第三方登入可 NULL
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Active  2=Suspended  3=Deleted
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_AccountType CHECK (AccountType IN (1, 2, 3)),
    CONSTRAINT CK_Users_Status CHECK (Status IN (1, 2, 3))
);
CREATE TABLE Merchants (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    UserId BIGINT NOT NULL,
    -- 初始建立者 (Owner)
    CompanyName NVARCHAR(200) NOT NULL,
    EnglishName NVARCHAR(200) NULL,
    -- 英文名稱
    TaxId NVARCHAR(20) NULL,
    IndustryType NVARCHAR(100) NULL,
    -- 行業類型（自由文字，前端可做下拉）
    ContactName NVARCHAR(100) NULL,
    Phone NVARCHAR(50) NULL,
    Fax NVARCHAR(50) NULL,
    CompanyEmail NVARCHAR(255) NULL,
    -- 公司信箱（與登入 Email 不同）
    Website NVARCHAR(500) NULL,
    Address NVARCHAR(300) NULL,
    EstablishedDate DATE NULL,
    VerificationStatus SMALLINT NOT NULL DEFAULT 2,
    -- Merchant first version uses only 2=Approved and 4=Suspended
    VerifiedAt DATETIME2 NULL,
    UpdatedByAdminId BIGINT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Merchants PRIMARY KEY (Id),
    CONSTRAINT FK_Merchants_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Merchants_AdminUser FOREIGN KEY (UpdatedByAdminId) REFERENCES Users(Id),
    CONSTRAINT CK_Merchants_VerificationStatus CHECK (VerificationStatus IN (2, 4))
);
CREATE TABLE KolProfiles (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    UserId BIGINT NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    RealName NVARCHAR(100) NULL,
    -- 審核 / 撥款用
    Phone NVARCHAR(50) NULL,
    LineContactId NVARCHAR(100) NULL,
    -- 聯絡用 LINE ID，不可作為 LINE OAuth / Messaging API userId
    Intro NVARCHAR(MAX) NULL,
    AcceptsCash BIT NOT NULL DEFAULT 1,
    -- 可合作：現金酬勞
    AcceptsBarter BIT NOT NULL DEFAULT 1,
    -- 可合作：體驗項目
    AcceptsCommission BIT NOT NULL DEFAULT 1,
    -- 可合作：導購分潤
    FollowersCount INT NULL,
    -- 快取值，實際數字以 KolSocialAccounts 為準
    VerificationStatus SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending  2=Approved  3=Rejected  4=Suspended
    VerifiedAt DATETIME2 NULL,
    VerifiedByAdminId BIGINT NULL,
    RejectionNote NVARCHAR(500) NULL,
    -- 審核退回原因（顯示於審核詳情頁）
    SuspensionNote NVARCHAR(500) NULL,
    -- 停權原因（選B）
    TermsAcceptedAt DATETIME2 NULL,
    -- KOL 平台使用條款同意時間（暫不做版本表）
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolProfiles PRIMARY KEY (Id),
    CONSTRAINT FK_KolProfiles_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_KolProfiles_AdminUser FOREIGN KEY (VerifiedByAdminId) REFERENCES Users(Id),
    CONSTRAINT CK_KolProfiles_VerificationStatus CHECK (VerificationStatus IN (1, 2, 3, 4))
);
CREATE TABLE KolReviewEvents (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    ActionType SMALLINT NOT NULL,
    -- 1=Submitted  2=Resubmitted  3=Approved  4=Returned
    FromStatus SMALLINT NULL,
    -- KolProfiles.VerificationStatus before this event; NULL for first submit
    ToStatus SMALLINT NOT NULL,
    -- KolProfiles.VerificationStatus after this event
    Comment NVARCHAR(1000) NULL,
    ActorUserId BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolReviewEvents PRIMARY KEY (Id),
    CONSTRAINT FK_KolReviewEvents_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT FK_KolReviewEvents_Actor FOREIGN KEY (ActorUserId) REFERENCES Users(Id),
    CONSTRAINT CK_KolReviewEvents_Action CHECK (ActionType IN (1, 2, 3, 4)),
    CONSTRAINT CK_KolReviewEvents_FromStatus CHECK (
        FromStatus IS NULL
        OR FromStatus IN (1, 2, 3, 4)
    ),
    CONSTRAINT CK_KolReviewEvents_ToStatus CHECK (ToStatus IN (1, 2, 3, 4))
);
CREATE TABLE KolCategories (
    -- KOL 類型（多選）
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    Category SMALLINT NOT NULL,
    -- 1=旅遊 2=旅行 3=時尚 4=美妝 5=居家生活 6=親子 7=寵物
    -- 8=遊戲 9=音樂 10=影視 11=藝術 12=書籍 13=科技 14=財經
    -- 15=教育 16=職場 17=健康 18=健身 19=運動 20=飲食
    -- 21=養生 22=公益 23=環保 24=政治 25=文化 26=跨界
    CONSTRAINT PK_KolCategories PRIMARY KEY (Id),
    CONSTRAINT FK_KolCategories_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolCategories_Kol_Cat UNIQUE (KolId, Category)
);
CREATE TABLE KolServiceAreas (
    -- KOL 可服務地區（多選）
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    AreaCode NVARCHAR(50) NOT NULL,
    -- e.g. Taipei / NewTaipei / Japan / Korea
    AreaName NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolServiceAreas PRIMARY KEY (Id),
    CONSTRAINT FK_KolServiceAreas_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolServiceAreas_Kol_Area UNIQUE (KolId, AreaCode)
);
CREATE TABLE KolLanguages (
    -- KOL 擅長語言（多選）
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    LanguageCode NVARCHAR(50) NOT NULL,
    -- e.g. zh-TW / taiwanese / ja / ko
    LanguageName NVARCHAR(100) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolLanguages PRIMARY KEY (Id),
    CONSTRAINT FK_KolLanguages_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolLanguages_Kol_Lang UNIQUE (KolId, LanguageCode)
);
CREATE TABLE KolSocialAccounts (
    -- 社群帳號（多筆，每個平台一筆）
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    Platform SMALLINT NOT NULL,
    -- 1=X 2=IG 3=FB 4=YT 5=Blog 6=小紅書 7=TikTok
    -- 8=中國抖音 9=Threads 10=Snapchat 11=WeChat
    AccountName NVARCHAR(200) NOT NULL,
    ProfileUrl NVARCHAR(500) NULL,
    -- KOL 輸入的社群網址，平台與帳號可由後端解析
    FollowersCount INT NULL,
    DataSource SMALLINT NOT NULL DEFAULT 2,
    -- 1=ApiSync 2=ManualInput
    VerificationStatus SMALLINT NOT NULL DEFAULT 2,
    -- 1=Verified 2=Unverified 3=NeedsConfirmation
    LastSyncAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolSocialAccounts PRIMARY KEY (Id),
    CONSTRAINT FK_KolSocialAccounts_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolSocialAccounts_Kol_Platform UNIQUE (KolId, Platform),
    CONSTRAINT CK_KolSocialAccounts_Platform CHECK (Platform IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)),
    CONSTRAINT CK_KolSocialAccounts_DataSource CHECK (DataSource IN (1, 2)),
    CONSTRAINT CK_KolSocialAccounts_VerifStatus CHECK (VerificationStatus IN (1, 2, 3))
);
CREATE TABLE KolBankAccounts (
    -- KOL 收款資料（每位 KOL 一筆有效資料）
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    AccountType SMALLINT NOT NULL DEFAULT 1,
    -- 1=個人 2=公司
    AccountName NVARCHAR(100) NOT NULL,
    -- 收款戶名
    BankCode NVARCHAR(10) NOT NULL,
    BankName NVARCHAR(100) NULL,
    BranchCode NVARCHAR(20) NULL,
    BranchName NVARCHAR(100) NULL,
    AccountNumberEncrypted NVARCHAR(500) NOT NULL,
    -- 銀行帳號加密儲存，顯示時遮蔽中間碼
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending 2=Verified 3=Rejected
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolBankAccounts PRIMARY KEY (Id),
    CONSTRAINT FK_KolBankAccounts_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolBankAccounts_Kol UNIQUE (KolId),
    -- 每位 KOL 只有一筆有效收款資料
    CONSTRAINT CK_KolBankAccounts_Type CHECK (AccountType IN (1, 2)),
    CONSTRAINT CK_KolBankAccounts_Status CHECK (Status IN (1, 2, 3))
);
CREATE TABLE KolTaxProfiles (
    -- KOL 稅務身分資料；銀行帳號仍由 KolBankAccounts 管理
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    IdentityType SMALLINT NOT NULL,
    -- 1=PersonalLocal  2=CompanyOrStudio
    ResidencyType SMALLINT NULL,
    -- 1=Local  2=ForeignOver183Days  3=ForeignUnder183Days
    PersonalName NVARCHAR(100) NULL,
    NationalIdEncrypted NVARCHAR(500) NULL,
    HouseholdAddress NVARCHAR(300) NULL,
    CompanyName NVARCHAR(200) NULL,
    TaxId NVARCHAR(20) NULL,
    RegisteredAddress NVARCHAR(300) NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending  2=Verified  3=Rejected
    VerifiedAt DATETIME2 NULL,
    VerifiedByAdminId BIGINT NULL,
    RejectionNote NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolTaxProfiles PRIMARY KEY (Id),
    CONSTRAINT FK_KolTaxProfiles_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT FK_KolTaxProfiles_Admin FOREIGN KEY (VerifiedByAdminId) REFERENCES Users(Id),
    CONSTRAINT UQ_KolTaxProfiles_Kol UNIQUE (KolId),
    CONSTRAINT CK_KolTaxProfiles_IdentityType CHECK (IdentityType IN (1, 2)),
    CONSTRAINT CK_KolTaxProfiles_ResidencyType CHECK (
        ResidencyType IS NULL
        OR ResidencyType IN (1, 2, 3)
    ),
    CONSTRAINT CK_KolTaxProfiles_Status CHECK (Status IN (1, 2, 3))
);
-- ================================================================
-- 2. RBAC
-- ================================================================
CREATE TABLE Roles (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Scope SMALLINT NOT NULL,
    -- 1=System  2=Merchant
    IsSystemReserved BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_Name_Scope UNIQUE (Name, Scope),
    CONSTRAINT CK_Roles_Scope CHECK (Scope IN (1, 2))
);
CREATE TABLE Permissions (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    Code NVARCHAR(200) NOT NULL,
    -- e.g. Merchant.Case.Create  Admin.Payout.Approve
    Description NVARCHAR(500) NULL,
    RiskLevel SMALLINT NOT NULL DEFAULT 1,
    -- 1=Normal  2=HighRisk
    CONSTRAINT PK_Permissions PRIMARY KEY (Id),
    CONSTRAINT UQ_Permissions_Code UNIQUE (Code),
    CONSTRAINT CK_Permissions_RiskLevel CHECK (RiskLevel IN (1, 2))
);
CREATE TABLE RolePermissions (
    RoleId BIGINT NOT NULL,
    PermissionId BIGINT NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_RolePermissions_Perm FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
);
CREATE TABLE UserRoles (
    -- 系統層級角色（Admin），Merchant 組織角色透過 MerchantMembers.RoleId 管理
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
-- ================================================================
-- 3. 業者組織與成員
-- ================================================================
CREATE TABLE MerchantMembers (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    RoleId BIGINT NOT NULL,
    -- 第一版每人一個 Merchant Scope 角色
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Active  2=Suspended  3=Removed
    JoinedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantMembers PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantMembers_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_MerchantMembers_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_MerchantMembers_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT UQ_MerchantMembers_Merchant_User UNIQUE (MerchantId, UserId),
    CONSTRAINT CK_MerchantMembers_Status CHECK (Status IN (1, 2, 3))
);
CREATE TABLE MerchantInvitations (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    InvitedByUserId BIGINT NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    RoleId BIGINT NOT NULL,
    TokenHash NVARCHAR(500) NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending  2=Accepted  3=Expired  4=Cancelled
    ExpiresAt DATETIME2 NOT NULL,
    AcceptedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantInvitations PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantInvitations_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_MerchantInvitations_Inviter FOREIGN KEY (InvitedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_MerchantInvitations_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT CK_MerchantInvitations_Status CHECK (Status IN (1, 2, 3, 4))
);
CREATE TABLE MerchantContacts (
    -- 業者多位聯絡窗口（可新增 / 刪除）
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Email NVARCHAR(255) NULL,
    Title NVARCHAR(100) NULL,
    -- 職稱
    Note NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantContacts PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantContacts_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id)
);
-- ================================================================
-- 4. 系統參數
-- ================================================================
CREATE TABLE SystemSettings (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    [Key] NVARCHAR(200) NOT NULL,
    -- e.g. case_opening_fee_rate
    Value NVARCHAR(MAX) NOT NULL,
    DefaultValue NVARCHAR(MAX) NOT NULL DEFAULT '',
    -- 還原預設時使用的值
    ValueType NVARCHAR(50) NOT NULL,
    -- string/number/percent/json/boolean
    [Group] NVARCHAR(100) NOT NULL,
    -- case_fee/commission/payout
    Description NVARCHAR(500) NULL,
    UpdatedByUserId BIGINT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SystemSettings PRIMARY KEY (Id),
    CONSTRAINT UQ_SystemSettings_Key UNIQUE ([Key]),
    CONSTRAINT FK_SystemSettings_User FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id)
);
CREATE TABLE SystemSettingLogs (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    SettingKey NVARCHAR(200) NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NOT NULL,
    Note NVARCHAR(500) NULL,
    ChangedByUserId BIGINT NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SystemSettingLogs PRIMARY KEY (Id),
    CONSTRAINT FK_SystemSettingLogs_User FOREIGN KEY (ChangedByUserId) REFERENCES Users(Id)
);
-- ================================================================
-- 5. 錢包與收益
-- ================================================================
CREATE TABLE MerchantWallets (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    AvailableAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    FrozenAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    TotalDepositedAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantWallets PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantWallets_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT UQ_MerchantWallets_Merchant UNIQUE (MerchantId)
);
CREATE TABLE MerchantWalletTransactions (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    Type SMALLINT NOT NULL,
    -- 1=OfflineDeposit  2=TaskBudgetFreeze  3=TaskBudgetRelease
    -- 4=TaskBudgetSettle  5=DisputeHold  6=ManualAdjustment
    Amount DECIMAL(12, 2) NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending  2=Approved  3=Rejected  4=Completed  5=Cancelled
    RelatedCaseId BIGINT NULL,
    -- FK 在 Cases 建立後加入
    Note NVARCHAR(500) NULL,
    CreatedByUserId BIGINT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantWalletTransactions PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantWalletTx_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_MerchantWalletTx_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_MerchantWalletTx_Type CHECK (Type IN (1, 2, 3, 4, 5, 6)),
    CONSTRAINT CK_MerchantWalletTx_Status CHECK (Status IN (1, 2, 3, 4, 5))
);
CREATE TABLE MerchantCreditWallets (
    -- 業者折扣金錢包；獨立於現金錢包，不可混入 MerchantWallets；第一版只能折抵案件開案費
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    AvailableAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    UsedAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    ExpiredAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    RevokedAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantCreditWallets PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantCreditWallets_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT UQ_MerchantCreditWallets_Merchant UNIQUE (MerchantId)
);
CREATE TABLE MerchantCreditTransactions (
    -- 折扣金交易流水；所有加值、扣回、折抵開案費、退回、到期都必須寫入
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    Type SMALLINT NOT NULL,
    -- 1=Grant  2=Use  3=Refund  4=Revoke  5=Expire  6=ManualAdjustment
    Amount DECIMAL(12, 2) NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 2,
    -- 1=Pending  2=Completed  3=Cancelled
    RelatedCaseId BIGINT NULL,
    -- FK 在 Cases 建立後加入
    ExpiredAt DATETIME2 NULL,
    Reason NVARCHAR(500) NULL,
    Note NVARCHAR(500) NULL,
    CreatedByUserId BIGINT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_MerchantCreditTransactions PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantCreditTx_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_MerchantCreditTx_User FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_MerchantCreditTx_Type CHECK (Type IN (1, 2, 3, 4, 5, 6)),
    CONSTRAINT CK_MerchantCreditTx_Status CHECK (Status IN (1, 2, 3))
);
CREATE TABLE KolWallets (
    -- 聚合餘額表，避免每次查詢都 SUM KolEarnings
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    PendingAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    -- 待驗收 / 待確認
    AvailableAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    -- 可提領
    PaidAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    -- 已撥款
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolWallets PRIMARY KEY (Id),
    CONSTRAINT FK_KolWallets_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolWallets_Kol UNIQUE (KolId)
);
CREATE TABLE KolEarnings (
    -- 每筆收益明細
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    -- FK 在 Cases 建立後加入
    TaskId BIGINT NOT NULL,
    -- FK 在 Tasks 建立後加入
    KolId BIGINT NOT NULL,
    SourceType SMALLINT NOT NULL,
    -- 1=CashReward  2=Commission  3=Adjustment
    GrossAmount DECIMAL(12, 2) NOT NULL,
    PlatformFeeAmount DECIMAL(12, 2) NULL,
    -- 公式待定
    NetAmount DECIMAL(12, 2) NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending  2=Hold  3=Available  4=Requested  5=Paid  6=Cancelled
    SettlementStatementId BIGINT NULL,
    -- FK 在 KolSettlementStatements 建立後加入
    PayoutRequestId BIGINT NULL,
    -- FK 在 PayoutRequests 建立後加入
    AvailableAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolEarnings PRIMARY KEY (Id),
    CONSTRAINT FK_KolEarnings_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT CK_KolEarnings_SourceType CHECK (SourceType IN (1, 2, 3)),
    CONSTRAINT CK_KolEarnings_Status CHECK (Status IN (1, 2, 3, 4, 5, 6))
);
CREATE TABLE PayoutRequests (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    SettlementStatementId BIGINT NULL,
    -- FK 在 KolSettlementStatements 建立後加入
    Amount DECIMAL(12, 2) NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Pending  2=Approved  3=Rejected  4=Paid  5=Cancelled
    DocumentStatus SMALLINT NOT NULL DEFAULT 1,
    -- 1=NotRequired  2=PendingUpload  3=Uploaded  4=Approved  5=Rejected
    DocumentReviewedByAdminId BIGINT NULL,
    DocumentReviewedAt DATETIME2 NULL,
    RequestedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReviewedByAdminId BIGINT NULL,
    PaidAt DATETIME2 NULL,
    Note NVARCHAR(500) NULL,
    CONSTRAINT PK_PayoutRequests PRIMARY KEY (Id),
    CONSTRAINT FK_PayoutRequests_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT FK_PayoutRequests_Admin FOREIGN KEY (ReviewedByAdminId) REFERENCES Users(Id),
    CONSTRAINT FK_PayoutRequests_DocAdmin FOREIGN KEY (DocumentReviewedByAdminId) REFERENCES Users(Id),
    CONSTRAINT CK_PayoutRequests_Status CHECK (Status IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_PayoutRequests_DocStatus CHECK (DocumentStatus IN (1, 2, 3, 4, 5))
);
CREATE TABLE KolSettlementStatements (
    -- KOL 月結批次 / 結算單快照
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolId BIGINT NOT NULL,
    SettlementMonth CHAR(7) NOT NULL,
    -- yyyy-MM
    TotalAmount DECIMAL(12, 2) NOT NULL,
    ItemCount INT NOT NULL DEFAULT 0,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Processing  2=Completed  3=Cancelled
    SettledAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolSettlementStatements PRIMARY KEY (Id),
    CONSTRAINT FK_KolSettlementStatements_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT UQ_KolSettlementStatements_Kol_Month UNIQUE (KolId, SettlementMonth),
    CONSTRAINT CK_KolSettlementStatements_Status CHECK (Status IN (1, 2, 3))
);
CREATE TABLE KolSettlementItems (
    -- 固定結算單與收益明細的關聯，避免歷史月結金額漂移
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    SettlementStatementId BIGINT NOT NULL,
    KolEarningId BIGINT NOT NULL,
    Amount DECIMAL(12, 2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolSettlementItems PRIMARY KEY (Id),
    CONSTRAINT FK_KolSettlementItems_Statement FOREIGN KEY (SettlementStatementId) REFERENCES KolSettlementStatements(Id),
    CONSTRAINT FK_KolSettlementItems_Earning FOREIGN KEY (KolEarningId) REFERENCES KolEarnings(Id),
    CONSTRAINT UQ_KolSettlementItems_Earning UNIQUE (KolEarningId)
);
ALTER TABLE KolEarnings
ADD CONSTRAINT FK_KolEarnings_Settlement FOREIGN KEY (SettlementStatementId) REFERENCES KolSettlementStatements(Id);
ALTER TABLE KolEarnings
ADD CONSTRAINT FK_KolEarnings_PayoutRequest FOREIGN KEY (PayoutRequestId) REFERENCES PayoutRequests(Id);
ALTER TABLE PayoutRequests
ADD CONSTRAINT FK_PayoutRequests_Settlement FOREIGN KEY (SettlementStatementId) REFERENCES KolSettlementStatements(Id);
-- ================================================================
-- 6. 案件主體
-- ================================================================
CREATE TABLE Cases (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    MerchantId BIGINT NOT NULL,
    CreatedByUserId BIGINT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    OfficialUrl NVARCHAR(500) NULL,
    City NVARCHAR(100) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    WantedKolCount INT NOT NULL,
    ApplicationDeadline DATETIME2 NOT NULL,
    SubmissionDeadline DATETIME2 NOT NULL,
    CashRewardAmount DECIMAL(12, 2) NOT NULL DEFAULT 0,
    IsCommissionEnabled BIT NOT NULL DEFAULT 0,
    CommissionRate DECIMAL(5, 2) NULL,
    -- 業者開案輸入的導購總佣金比例；需 >= 平台抽成比例 + KOL 最低分潤比例
    CookieDays INT NULL,
    DeliverableDescription NVARCHAR(MAX) NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Draft  2=Recruiting  3=RecruitmentClosed  4=InProgress
    -- 5=Completed  6=Settled  7=Cancelled
    RecruitmentStatus SMALLINT NOT NULL DEFAULT 1,
    -- 1=NotOpen  2=Open  3=Closed  4=Paused
    AutoExecutionThresholdRate DECIMAL(5, 2) NOT NULL,
    -- 快照，來自 SystemSettings
    AutoExecutionThresholdCount INT NOT NULL,
    -- Ceiling(WantedKolCount × Rate)
    ApplicationCount INT NOT NULL DEFAULT 0,
    -- 快取
    ApprovedAssignmentCount INT NOT NULL DEFAULT 0,
    -- 快取
    PublishedAt DATETIME2 NULL,
    StartedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    SettledAt DATETIME2 NULL,
    CancelledAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Cases PRIMARY KEY (Id),
    CONSTRAINT FK_Cases_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_Cases_Creator FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_Cases_Status CHECK (Status IN (1, 2, 3, 4, 5, 6, 7)),
    CONSTRAINT CK_Cases_RecruitStatus CHECK (RecruitmentStatus IN (1, 2, 3, 4))
);
-- 加入 Cases 建立後才能加的 FK
ALTER TABLE MerchantWalletTransactions
ADD CONSTRAINT FK_MerchantWalletTx_Case FOREIGN KEY (RelatedCaseId) REFERENCES Cases(Id);
ALTER TABLE MerchantCreditTransactions
ADD CONSTRAINT FK_MerchantCreditTx_Case FOREIGN KEY (RelatedCaseId) REFERENCES Cases(Id);
CREATE TABLE CaseBudgetSnapshots (
    -- 發布時的預算快照，案件修改不追溯
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    RewardAmountPerKol DECIMAL(12, 2) NOT NULL,
    WantedKolCount INT NOT NULL,
    RewardSubtotal DECIMAL(12, 2) NOT NULL,
    -- 單價 × 數量
    FeeItems NVARCHAR(MAX) NOT NULL,
    -- JSON: [{code, rate, base, amount}]，包含 KOL 服務費與固定開案費計算明細
    EstimatedFrozenAmount DECIMAL(12, 2) NOT NULL,
    -- 發布時凍結金額 = (RewardAmountPerKol * WantedKolCount * KolServiceFeeRate) + CaseOpeningFeeAmount
    SettingsSnapshot NVARCHAR(MAX) NOT NULL,
    -- JSON: Admin 參數快照
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_CaseBudgetSnapshots PRIMARY KEY (Id),
    CONSTRAINT FK_CaseBudgetSnapshots_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id)
);
-- ================================================================
-- 7. 案件條件與附件
-- ================================================================
CREATE TABLE CasePlatforms (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    Platform SMALLINT NOT NULL,
    -- 1=Instagram  2=Facebook  3=YouTube  4=TikTok  5=Threads  6=Blog
    CONSTRAINT PK_CasePlatforms PRIMARY KEY (Id),
    CONSTRAINT FK_CasePlatforms_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT CK_CasePlatforms_Plat CHECK (Platform IN (1, 2, 3, 4, 5, 6))
);
CREATE TABLE CaseCategories (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    Category SMALLINT NOT NULL,
    -- 1=Beauty  2=Travel  3=Fashion  4=Parenting  5=Pet  6=Health  ...（最多 10 個）
    CONSTRAINT PK_CaseCategories PRIMARY KEY (Id),
    CONSTRAINT FK_CaseCategories_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id)
);
CREATE TABLE CaseLanguages (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    Language SMALLINT NOT NULL,
    -- 1=ZhTw  2=En  3=Ja  4=Ko
    CONSTRAINT PK_CaseLanguages PRIMARY KEY (Id),
    CONSTRAINT FK_CaseLanguages_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id)
);
CREATE TABLE CaseRequirements (
    -- KOL 條件為提示，不阻擋報名
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    MinFollowers INT NULL,
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT PK_CaseRequirements PRIMARY KEY (Id),
    CONSTRAINT FK_CaseRequirements_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id)
);
CREATE TABLE CaseBarterItems (
    -- 體驗項目，不估值進預算
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Quantity INT NULL,
    Note NVARCHAR(500) NULL,
    CONSTRAINT PK_CaseBarterItems PRIMARY KEY (Id),
    CONSTRAINT FK_CaseBarterItems_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id)
);
CREATE TABLE Files (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    UploadedByUserId BIGINT NOT NULL,
    FileName NVARCHAR(500) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    FileSize INT NOT NULL,
    MimeType NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Files PRIMARY KEY (Id),
    CONSTRAINT FK_Files_User FOREIGN KEY (UploadedByUserId) REFERENCES Users(Id)
);
CREATE TABLE CaseAttachments (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    FileId BIGINT NOT NULL,
    Type SMALLINT NOT NULL,
    -- 1=ReferenceMaterial  2=Script  3=Contract  4=Other
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_CaseAttachments PRIMARY KEY (Id),
    CONSTRAINT FK_CaseAttachments_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT FK_CaseAttachments_File FOREIGN KEY (FileId) REFERENCES Files(Id),
    CONSTRAINT CK_CaseAttachments_Type CHECK (Type IN (1, 2, 3, 4))
);
CREATE TABLE KolTaxDocuments (
    -- KOL 稅務與收款附件
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    KolTaxProfileId BIGINT NOT NULL,
    FileId BIGINT NOT NULL,
    DocumentType SMALLINT NOT NULL,
    -- 1=IdFront  2=IdBack  3=BankbookCopy  4=CompanyBankbookCopy  5=Other
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_KolTaxDocuments PRIMARY KEY (Id),
    CONSTRAINT FK_KolTaxDocuments_Profile FOREIGN KEY (KolTaxProfileId) REFERENCES KolTaxProfiles(Id),
    CONSTRAINT FK_KolTaxDocuments_File FOREIGN KEY (FileId) REFERENCES Files(Id),
    CONSTRAINT CK_KolTaxDocuments_Type CHECK (DocumentType IN (1, 2, 3, 4, 5))
);
CREATE TABLE PayoutRequestDocuments (
    -- 提領文件：個人戶勞報單、公司戶發票影本
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    PayoutRequestId BIGINT NOT NULL,
    FileId BIGINT NOT NULL,
    DocumentType SMALLINT NOT NULL,
    -- 1=LaborRemunerationForm  2=InvoiceCopy
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Uploaded  2=Approved  3=Rejected
    ReviewedByAdminId BIGINT NULL,
    ReviewedAt DATETIME2 NULL,
    RejectReason NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_PayoutRequestDocuments PRIMARY KEY (Id),
    CONSTRAINT FK_PayoutRequestDocuments_Payout FOREIGN KEY (PayoutRequestId) REFERENCES PayoutRequests(Id),
    CONSTRAINT FK_PayoutRequestDocuments_File FOREIGN KEY (FileId) REFERENCES Files(Id),
    CONSTRAINT FK_PayoutRequestDocuments_Admin FOREIGN KEY (ReviewedByAdminId) REFERENCES Users(Id),
    CONSTRAINT CK_PayoutRequestDocuments_Type CHECK (DocumentType IN (1, 2)),
    CONSTRAINT CK_PayoutRequestDocuments_Status CHECK (Status IN (1, 2, 3))
);
-- ================================================================
-- 8. 報名
-- ================================================================
CREATE TABLE CaseApplications (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    KolId BIGINT NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Applied  2=Accepted  3=PendingReconfirmation  4=Rejected  5=Cancelled  6=Invalid
    Message NVARCHAR(MAX) NULL,
    IsRequirementMatched BIT NOT NULL DEFAULT 1,
    MismatchReasons NVARCHAR(MAX) NULL,
    -- JSON: ["followers_not_enough"]
    ReconfirmedAt DATETIME2 NULL,
    AppliedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReviewedAt DATETIME2 NULL,
    ReviewedByUserId BIGINT NULL,
    CONSTRAINT PK_CaseApplications PRIMARY KEY (Id),
    CONSTRAINT FK_CaseApplications_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT FK_CaseApplications_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT FK_CaseApplications_Reviewer FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id),
    CONSTRAINT UQ_CaseApplications_Case_Kol UNIQUE (CaseId, KolId),
    -- 同一案件同一 KOL 只能報名一次
    CONSTRAINT CK_CaseApplications_Status CHECK (Status IN (1, 2, 3, 4, 5, 6))
);
-- ================================================================
-- 9. 執行任務
--    Cases 發布時建立 WantedKolCount 筆，初始 Status=PendingMatch
--    Application 被接受後，Task 綁定 KolId + ApplicationId
-- ================================================================
CREATE TABLE Tasks (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    KolId BIGINT NULL,
    -- NULL = 尚未綁定 (PendingMatch)
    ApplicationId BIGINT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=PendingMatch  2=PendingExecution  3=InProgress  4=UnderReview
    -- 5=RevisionRequested  6=Completed  7=Incomplete  8=Cancelled
    StartedAt DATETIME2 NULL,
    SubmittedAt DATETIME2 NULL,
    CompletedAt DATETIME2 NULL,
    CancelledAt DATETIME2 NULL,
    CONSTRAINT PK_Tasks PRIMARY KEY (Id),
    CONSTRAINT FK_Tasks_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT FK_Tasks_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT FK_Tasks_Application FOREIGN KEY (ApplicationId) REFERENCES CaseApplications(Id),
    CONSTRAINT CK_Tasks_Status CHECK (Status IN (1, 2, 3, 4, 5, 6, 7, 8))
);
-- Tasks 建立後補上 KolEarnings FK
ALTER TABLE KolEarnings
ADD CONSTRAINT FK_KolEarnings_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id);
ALTER TABLE KolEarnings
ADD CONSTRAINT FK_KolEarnings_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id);
-- ================================================================
-- 10. 成果提交與驗收
-- ================================================================
CREATE TABLE Submissions (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    TaskId BIGINT NOT NULL,
    KolId BIGINT NOT NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Submitted  2=RevisionRequested  3=Approved  4=Rejected  5=Overdue  6=Disputed
    IsAutoApproved BIT NOT NULL DEFAULT 0,
    Note NVARCHAR(MAX) NULL,
    ReviewDeadlineAt DATETIME2 NOT NULL,
    -- SubmittedAt + 14 天，KOL 重提後重算
    SubmittedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReviewedAt DATETIME2 NULL,
    ReviewedByUserId BIGINT NULL,
    CONSTRAINT PK_Submissions PRIMARY KEY (Id),
    CONSTRAINT FK_Submissions_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    CONSTRAINT FK_Submissions_Kol FOREIGN KEY (KolId) REFERENCES KolProfiles(Id),
    CONSTRAINT FK_Submissions_Reviewer FOREIGN KEY (ReviewedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_Submissions_Status CHECK (Status IN (1, 2, 3, 4, 5, 6))
);
CREATE TABLE SubmissionItems (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    SubmissionId BIGINT NOT NULL,
    Platform SMALLINT NULL,
    -- 1=Instagram  2=Facebook  3=YouTube  4=TikTok  5=Threads  6=Blog
    Url NVARCHAR(500) NULL,
    FileId BIGINT NULL,
    Note NVARCHAR(500) NULL,
    CONSTRAINT PK_SubmissionItems PRIMARY KEY (Id),
    CONSTRAINT FK_SubmissionItems_Sub FOREIGN KEY (SubmissionId) REFERENCES Submissions(Id),
    CONSTRAINT FK_SubmissionItems_File FOREIGN KEY (FileId) REFERENCES Files(Id),
    CONSTRAINT CK_SubmissionItems_Platform CHECK (Platform IN (1, 2, 3, 4, 5, 6))
);
-- ================================================================
-- 11. 評分
-- ================================================================
CREATE TABLE ReviewCriteria (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    TargetRole SMALLINT NOT NULL,
    -- 1=Merchant  2=Kol
    Code NVARCHAR(100) NOT NULL,
    Label NVARCHAR(200) NOT NULL,
    SortOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_ReviewCriteria PRIMARY KEY (Id),
    CONSTRAINT UQ_ReviewCriteria_Code UNIQUE (Code),
    CONSTRAINT CK_ReviewCriteria_Role CHECK (TargetRole IN (1, 2))
);
CREATE TABLE Reviews (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    TaskId BIGINT NOT NULL,
    ReviewerUserId BIGINT NOT NULL,
    RevieweeUserId BIGINT NOT NULL,
    ReviewerRole SMALLINT NOT NULL,
    -- 1=Merchant  2=Kol
    RevieweeRole SMALLINT NOT NULL,
    -- 1=Merchant  2=Kol
    OverallRating TINYINT NOT NULL,
    -- 1-5
    Comment NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Reviews PRIMARY KEY (Id),
    CONSTRAINT FK_Reviews_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT FK_Reviews_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    CONSTRAINT FK_Reviews_Reviewer FOREIGN KEY (ReviewerUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Reviews_Reviewee FOREIGN KEY (RevieweeUserId) REFERENCES Users(Id),
    CONSTRAINT CK_Reviews_ReviewerRole CHECK (ReviewerRole IN (1, 2)),
    CONSTRAINT CK_Reviews_RevieweeRole CHECK (RevieweeRole IN (1, 2)),
    CONSTRAINT CK_Reviews_Rating CHECK (
        OverallRating BETWEEN 1 AND 5
    )
);
CREATE TABLE ReviewScores (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    ReviewId BIGINT NOT NULL,
    CriteriaCode NVARCHAR(100) NOT NULL,
    Rating TINYINT NOT NULL,
    -- 1-5
    CONSTRAINT PK_ReviewScores PRIMARY KEY (Id),
    CONSTRAINT FK_ReviewScores_Review FOREIGN KEY (ReviewId) REFERENCES Reviews(Id),
    CONSTRAINT CK_ReviewScores_Rating CHECK (
        Rating BETWEEN 1 AND 5
    )
);
-- ================================================================
-- 12. 爭議
-- ================================================================
CREATE TABLE Disputes (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NOT NULL,
    TaskId BIGINT NOT NULL,
    OpenedByUserId BIGINT NOT NULL,
    AgainstUserId BIGINT NULL,
    Reason NVARCHAR(500) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status SMALLINT NOT NULL DEFAULT 1,
    -- 1=Open  2=UnderReview  3=ResolvedForMerchant  4=ResolvedForKol
    -- 5=ResolvedCompromise  6=Cancelled
    ResolvedByAdminId BIGINT NULL,
    ResolutionNote NVARCHAR(MAX) NULL,
    OpenedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ResolvedAt DATETIME2 NULL,
    CONSTRAINT PK_Disputes PRIMARY KEY (Id),
    CONSTRAINT FK_Disputes_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT FK_Disputes_Task FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    CONSTRAINT FK_Disputes_Opener FOREIGN KEY (OpenedByUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Disputes_Against FOREIGN KEY (AgainstUserId) REFERENCES Users(Id),
    CONSTRAINT FK_Disputes_Admin FOREIGN KEY (ResolvedByAdminId) REFERENCES Users(Id),
    CONSTRAINT CK_Disputes_Status CHECK (Status IN (1, 2, 3, 4, 5, 6))
);
CREATE TABLE DisputeMessages (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    DisputeId BIGINT NOT NULL,
    SenderUserId BIGINT NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DisputeMessages PRIMARY KEY (Id),
    CONSTRAINT FK_DisputeMessages_Dispute FOREIGN KEY (DisputeId) REFERENCES Disputes(Id),
    CONSTRAINT FK_DisputeMessages_Sender FOREIGN KEY (SenderUserId) REFERENCES Users(Id)
);
CREATE TABLE DisputeAttachments (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    DisputeId BIGINT NOT NULL,
    FileId BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_DisputeAttachments PRIMARY KEY (Id),
    CONSTRAINT FK_DisputeAttachments_Dispute FOREIGN KEY (DisputeId) REFERENCES Disputes(Id),
    CONSTRAINT FK_DisputeAttachments_File FOREIGN KEY (FileId) REFERENCES Files(Id)
);
-- ================================================================
-- 13. 活動紀錄與通知
-- ================================================================
CREATE TABLE ActivityLogs (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    CaseId BIGINT NULL,
    TargetType NVARCHAR(100) NULL,
    -- e.g. Cases / KolBankAccounts / PayoutRequests / Roles
    TargetId BIGINT NULL,
    ActorUserId BIGINT NULL,
    -- 系統操作可 NULL
    Action NVARCHAR(200) NOT NULL,
    -- e.g. CaseCreated  StatusChanged  ApplicationSubmitted  SubmissionApproved
    BeforeData NVARCHAR(MAX) NULL,
    -- JSON
    AfterData NVARCHAR(MAX) NULL,
    -- JSON
    IpAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    Note NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ActivityLogs PRIMARY KEY (Id),
    CONSTRAINT FK_ActivityLogs_Case FOREIGN KEY (CaseId) REFERENCES Cases(Id),
    CONSTRAINT FK_ActivityLogs_Actor FOREIGN KEY (ActorUserId) REFERENCES Users(Id)
);
CREATE TABLE Notifications (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    UserId BIGINT NOT NULL,
    Type NVARCHAR(100) NOT NULL,
    Title NVARCHAR(300) NOT NULL,
    Body NVARCHAR(MAX) NULL,
    RelatedType NVARCHAR(100) NULL,
    -- Cases / Disputes / PayoutRequests
    RelatedId BIGINT NULL,
    ReadAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Notifications PRIMARY KEY (Id),
    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);
CREATE TABLE NotificationPreferences (
    -- 通用通知偏好；KOL 使用 OwnerType=User，業者端公司層級使用 OwnerType=Merchant
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    OwnerType SMALLINT NOT NULL,
    -- 1=User  2=Merchant
    OwnerUserId BIGINT NULL,
    MerchantId BIGINT NULL,
    EventType NVARCHAR(100) NOT NULL,
    -- e.g. KolSelectedForTask / DeadlineReminder / SubmissionSubmitted / WalletChanged / SystemMaintenance
    Channel SMALLINT NOT NULL,
    -- 1=InApp  2=Email  3=Line
    IsEnabled BIT NOT NULL DEFAULT 1,
    IsMandatory BIT NOT NULL DEFAULT 0,
    UpdatedByUserId BIGINT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_NotificationPreferences PRIMARY KEY (Id),
    CONSTRAINT FK_NotificationPreferences_User FOREIGN KEY (OwnerUserId) REFERENCES Users(Id),
    CONSTRAINT FK_NotificationPreferences_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_NotificationPreferences_UpdatedBy FOREIGN KEY (UpdatedByUserId) REFERENCES Users(Id),
    CONSTRAINT CK_NotificationPreferences_OwnerType CHECK (OwnerType IN (1, 2)),
    CONSTRAINT CK_NotificationPreferences_Channel CHECK (Channel IN (1, 2, 3)),
    CONSTRAINT CK_NotificationPreferences_Owner CHECK (
        (
            OwnerType = 1
            AND OwnerUserId IS NOT NULL
            AND MerchantId IS NULL
        )
        OR (
            OwnerType = 2
            AND MerchantId IS NOT NULL
            AND OwnerUserId IS NULL
        )
    ),
    CONSTRAINT CK_NotificationPreferences_Mandatory CHECK (
        IsMandatory = 0
        OR IsEnabled = 1
    )
);
-- ================================================================
-- 14. 常用索引
-- ================================================================
CREATE INDEX IX_Merchants_UserId ON Merchants (UserId);
CREATE INDEX IX_KolProfiles_UserId ON KolProfiles (UserId);
CREATE INDEX IX_KolReviewEvents_KolCreated ON KolReviewEvents (KolId, CreatedAt DESC);
CREATE INDEX IX_KolReviewEvents_ActionCreated ON KolReviewEvents (ActionType, CreatedAt DESC);
CREATE INDEX IX_KolCategories_KolId ON KolCategories (KolId);
CREATE INDEX IX_KolServiceAreas_KolId ON KolServiceAreas (KolId);
CREATE INDEX IX_KolLanguages_KolId ON KolLanguages (KolId);
CREATE INDEX IX_KolSocialAccounts_KolId ON KolSocialAccounts (KolId);
CREATE INDEX IX_KolBankAccounts_KolId ON KolBankAccounts (KolId);
CREATE INDEX IX_KolTaxProfiles_KolId ON KolTaxProfiles (KolId);
CREATE INDEX IX_KolTaxDocuments_Profile ON KolTaxDocuments (KolTaxProfileId);
CREATE INDEX IX_MerchantMembers_Merchant ON MerchantMembers (MerchantId);
CREATE INDEX IX_MerchantMembers_User ON MerchantMembers (UserId);
CREATE INDEX IX_Cases_MerchantId ON Cases (MerchantId);
CREATE INDEX IX_Cases_Status ON Cases (Status);
CREATE INDEX IX_CaseApplications_CaseId ON CaseApplications (CaseId);
CREATE INDEX IX_CaseApplications_KolId ON CaseApplications (KolId);
CREATE INDEX IX_Tasks_CaseId ON Tasks (CaseId);
CREATE INDEX IX_Tasks_KolId ON Tasks (KolId);
CREATE INDEX IX_Submissions_TaskId ON Submissions (TaskId);
CREATE INDEX IX_Submissions_ReviewDeadline ON Submissions (ReviewDeadlineAt)
WHERE Status = 1;
-- 自動驗收排程用
CREATE INDEX IX_KolEarnings_KolId ON KolEarnings (KolId);
CREATE INDEX IX_KolEarnings_TaskId ON KolEarnings (TaskId);
CREATE INDEX IX_KolEarnings_Settlement ON KolEarnings (SettlementStatementId);
CREATE INDEX IX_KolEarnings_PayoutRequest ON KolEarnings (PayoutRequestId);
CREATE INDEX IX_KolSettlementStatements_KolMonth ON KolSettlementStatements (KolId, SettlementMonth);
CREATE INDEX IX_KolSettlementItems_Statement ON KolSettlementItems (SettlementStatementId);
CREATE INDEX IX_PayoutRequests_KolId ON PayoutRequests (KolId);
CREATE INDEX IX_PayoutRequestDocuments_Payout ON PayoutRequestDocuments (PayoutRequestId);
CREATE INDEX IX_ActivityLogs_CaseId ON ActivityLogs (CaseId);
CREATE INDEX IX_ActivityLogs_Target ON ActivityLogs (TargetType, TargetId);
CREATE INDEX IX_Notifications_UserId ON Notifications (UserId);
CREATE INDEX IX_Notifications_Unread ON Notifications (UserId, ReadAt)
WHERE ReadAt IS NULL;
CREATE UNIQUE INDEX UX_NotificationPreferences_User ON NotificationPreferences (OwnerUserId, EventType, Channel)
WHERE OwnerType = 1
    AND OwnerUserId IS NOT NULL;
CREATE UNIQUE INDEX UX_NotificationPreferences_Merchant ON NotificationPreferences (MerchantId, EventType, Channel)
WHERE OwnerType = 2
    AND MerchantId IS NOT NULL;
CREATE INDEX IX_NotificationPreferences_OwnerType ON NotificationPreferences (OwnerType, EventType);
CREATE INDEX IX_MerchantWalletTx_Merchant ON MerchantWalletTransactions (MerchantId);
CREATE INDEX IX_MerchantCreditWallets_Merchant ON MerchantCreditWallets (MerchantId);
CREATE INDEX IX_MerchantCreditTx_Merchant ON MerchantCreditTransactions (MerchantId);
CREATE INDEX IX_MerchantCreditTx_Case ON MerchantCreditTransactions (RelatedCaseId)
WHERE RelatedCaseId IS NOT NULL;
CREATE INDEX IX_MerchantCreditTx_Expired ON MerchantCreditTransactions (ExpiredAt)
WHERE ExpiredAt IS NOT NULL
    AND Status = 2;
CREATE INDEX IX_Disputes_CaseId ON Disputes (CaseId);
CREATE INDEX IX_MerchantContacts_MerchantId ON MerchantContacts (MerchantId);
-- ================================================================
-- 15. 預設系統參數
-- ================================================================
INSERT INTO SystemSettings ([Key], Value, ValueType, [Group], Description)
VALUES (
        'case_opening_fee_amount',
        '1000',
        'number',
        'case_fee',
        N'案件固定開案費；案件發布預估凍結金額使用'
    ),
    (
        'kol_service_fee_rate',
        '0',
        'percent',
        'case_fee',
        N'KOL 服務費率；案件發布預估凍結金額使用'
    ),
    (
        'affiliate_platform_commission_rate',
        '0',
        'percent',
        'commission',
        N'導購平台抽成比例；平台固定保留此比例'
    ),
    (
        'affiliate_kol_min_commission_rate',
        '0',
        'percent',
        'commission',
        N'KOL 最低分潤比例；與平台抽成比例合計為業者佣金最低比例'
    ),
    (
        'case_auto_execution_threshold_rate',
        '50',
        'percent',
        'case',
        N'案件自動執行門檻；招募截止時錄取人數需達預計招募人數比例'
    ),
    (
        'kol_payout_min_amount',
        '1000',
        'number',
        'payout',
        N'KOL 最低提領門檻；金額需 >= 此值才可提領'
    );