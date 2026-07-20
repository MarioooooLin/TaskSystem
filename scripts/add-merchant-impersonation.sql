-- ================================================================
-- 管理者代理登入業者端 — 資料庫異動 Script
-- 目標：建立一次性票證資料表、索引與後台權限 Admin.Merchant.Impersonate。
-- 特性：可重複執行，已存在則略過；執行失敗自動 rollback。
-- 注意：本 Script 不會直接建立或修改任何測試 token / 密碼。
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
-- 1. 建立一次性代理登入票證資料表
IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = 'MerchantImpersonationTickets'
        AND type = 'U'
) BEGIN CREATE TABLE MerchantImpersonationTickets (
    Id BIGINT IDENTITY(1, 1) NOT NULL,
    TokenHash VARCHAR(64) NOT NULL,
    -- SHA-256 hex，唯一且不可逆
    MerchantId BIGINT NOT NULL,
    AdminUserId BIGINT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    ExpiresAtUtc DATETIME2 NOT NULL,
    UsedAtUtc DATETIME2 NULL,
    CreatedIp NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    CONSTRAINT PK_MerchantImpersonationTickets PRIMARY KEY (Id),
    CONSTRAINT FK_MerchantImpersonationTickets_Merchant FOREIGN KEY (MerchantId) REFERENCES Merchants(Id),
    CONSTRAINT FK_MerchantImpersonationTickets_Admin FOREIGN KEY (AdminUserId) REFERENCES Users(Id)
);
END -- 2. 索引
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('MerchantImpersonationTickets')
        AND name = 'UQ_MerchantImpersonationTickets_TokenHash'
) BEGIN CREATE UNIQUE INDEX UQ_MerchantImpersonationTickets_TokenHash ON MerchantImpersonationTickets(TokenHash);
END IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('MerchantImpersonationTickets')
        AND name = 'IX_MerchantImpersonationTickets_MerchantId'
) BEGIN CREATE INDEX IX_MerchantImpersonationTickets_MerchantId ON MerchantImpersonationTickets(MerchantId);
END IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('MerchantImpersonationTickets')
        AND name = 'IX_MerchantImpersonationTickets_AdminUserId'
) BEGIN CREATE INDEX IX_MerchantImpersonationTickets_AdminUserId ON MerchantImpersonationTickets(AdminUserId);
END IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('MerchantImpersonationTickets')
        AND name = 'IX_MerchantImpersonationTickets_ExpiresUsed'
) BEGIN CREATE INDEX IX_MerchantImpersonationTickets_ExpiresUsed ON MerchantImpersonationTickets(ExpiresAtUtc, UsedAtUtc);
END -- 3. 建立後台權限
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Admin.Merchant.Impersonate'
) BEGIN
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES (
        'Admin.Merchant.Impersonate',
        N'代理登入業者端（唯讀模式）',
        2
    );
-- HighRisk
END -- 4. 將權限綁定到「系統管理員」系統保留角色（可依營運需求調整為手動指派給其他角色）
DECLARE @PermissionId BIGINT;
DECLARE @SystemAdminRoleId BIGINT;
SELECT @PermissionId = Id
FROM Permissions
WHERE Code = 'Admin.Merchant.Impersonate';
SELECT @SystemAdminRoleId = Id
FROM Roles
WHERE Name = N'系統管理員'
    AND Scope = 1;
IF @PermissionId IS NOT NULL
AND @SystemAdminRoleId IS NOT NULL
AND NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @SystemAdminRoleId
        AND PermissionId = @PermissionId
) BEGIN
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@SystemAdminRoleId, @PermissionId);
END COMMIT TRANSACTION;
SELECT N'MerchantImpersonationTickets 資料表、索引與 Admin.Merchant.Impersonate 權限已就緒。' AS Result;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;