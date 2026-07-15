-- ================================================================
-- Admin Dispute Permissions Seed
-- 目標：為現有 admin@ttm.com.tw 系統管理員帳號建立並綁定
--        Admin.Dispute.View / Admin.Dispute.Resolve 權限。
-- 特性：可重複執行，已存在則略過。
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @AdminUserId BIGINT;
SELECT TOP 1 @AdminUserId = Id
FROM Users
WHERE Email = 'admin@ttm.com.tw'
    AND AccountType = 1;
-- Admin
IF @AdminUserId IS NULL BEGIN THROW 50001,
N'找不到 admin@ttm.com.tw 系統管理員帳號。',
1;
END;
BEGIN TRY BEGIN TRANSACTION;
-- 1. 建立異議處理相關 Permissions
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Admin.Dispute.View'
) BEGIN
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Admin.Dispute.View', N'檢視異議處理', 1);
END;
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Admin.Dispute.Resolve'
) BEGIN
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Admin.Dispute.Resolve', N'處理並結案異議', 2);
-- 高風險：影響任務與示意帳務
END;
DECLARE @ViewPermissionId BIGINT;
DECLARE @ResolvePermissionId BIGINT;
SELECT @ViewPermissionId = Id
FROM Permissions
WHERE Code = 'Admin.Dispute.View';
SELECT @ResolvePermissionId = Id
FROM Permissions
WHERE Code = 'Admin.Dispute.Resolve';
-- 2. 建立系統管理員角色（若尚未存在）
IF NOT EXISTS (
    SELECT 1
    FROM Roles
    WHERE Name = N'系統管理員'
        AND Scope = 1
) BEGIN
INSERT INTO Roles (
        Name,
        Description,
        Scope,
        IsSystemReserved,
        IsActive
    )
VALUES (N'系統管理員', N'擁有所有後台管理權限的系統保留角色', 1, 1, 1);
END;
DECLARE @RoleId BIGINT;
SELECT @RoleId = Id
FROM Roles
WHERE Name = N'系統管理員'
    AND Scope = 1;
-- 3. 綁定權限到角色
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @RoleId
        AND PermissionId = @ViewPermissionId
) BEGIN
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@RoleId, @ViewPermissionId);
END;
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @RoleId
        AND PermissionId = @ResolvePermissionId
) BEGIN
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@RoleId, @ResolvePermissionId);
END;
-- 4. 指派角色給 admin@ttm.com.tw
IF NOT EXISTS (
    SELECT 1
    FROM UserRoles
    WHERE UserId = @AdminUserId
        AND RoleId = @RoleId
) BEGIN
INSERT INTO UserRoles (UserId, RoleId)
VALUES (@AdminUserId, @RoleId);
END;
COMMIT TRANSACTION;
SELECT N'Admin.Dispute 權限與系統管理員角色關聯完成。' AS Result;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;