-- ================================================================
-- Seed: Merchant Case Permissions
-- 目標：為業者端建立預設角色與 Merchant.Case.* 權限的對應。
-- 特性：idempotent，可重複執行；已存在則略過。
-- 預設角色：Owner / Admin / Member
-- ================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY BEGIN TRANSACTION;
-- 1. 建立 Merchant.Case 相關 Permissions
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Merchant.Case.View'
)
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Merchant.Case.View', N'查看案件', 1);
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Merchant.Case.Manage'
)
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Merchant.Case.Manage', N'新增與編輯案件', 1);
IF NOT EXISTS (
    SELECT 1
    FROM Permissions
    WHERE Code = 'Merchant.Case.Publish'
)
INSERT INTO Permissions (Code, Description, RiskLevel)
VALUES ('Merchant.Case.Publish', N'發布案件並鎖定預算', 2);
DECLARE @ViewId BIGINT,
    @ManageId BIGINT,
    @PublishId BIGINT;
SELECT @ViewId = Id
FROM Permissions
WHERE Code = 'Merchant.Case.View';
SELECT @ManageId = Id
FROM Permissions
WHERE Code = 'Merchant.Case.Manage';
SELECT @PublishId = Id
FROM Permissions
WHERE Code = 'Merchant.Case.Publish';
-- 2. 建立預設 Merchant 角色（若尚未存在）
IF NOT EXISTS (
    SELECT 1
    FROM Roles
    WHERE Name = N'Owner'
        AND Scope = 2
)
INSERT INTO Roles (
        Name,
        Description,
        Scope,
        IsSystemReserved,
        IsActive
    )
VALUES (N'Owner', N'業者擁有人，具備所有權限', 2, 1, 1);
IF NOT EXISTS (
    SELECT 1
    FROM Roles
    WHERE Name = N'Admin'
        AND Scope = 2
)
INSERT INTO Roles (
        Name,
        Description,
        Scope,
        IsSystemReserved,
        IsActive
    )
VALUES (N'Admin', N'業者管理員，可管理案件、KOL、錢包與企業資料', 2, 1, 1);
IF NOT EXISTS (
    SELECT 1
    FROM Roles
    WHERE Name = N'Member'
        AND Scope = 2
)
INSERT INTO Roles (
        Name,
        Description,
        Scope,
        IsSystemReserved,
        IsActive
    )
VALUES (N'Member', N'業者成員，僅具備查看權限', 2, 1, 1);
DECLARE @OwnerRoleId BIGINT,
    @AdminRoleId BIGINT,
    @MemberRoleId BIGINT;
SELECT @OwnerRoleId = Id
FROM Roles
WHERE Name = N'Owner'
    AND Scope = 2;
SELECT @AdminRoleId = Id
FROM Roles
WHERE Name = N'Admin'
    AND Scope = 2;
SELECT @MemberRoleId = Id
FROM Roles
WHERE Name = N'Member'
    AND Scope = 2;
-- 3. Owner：全部案件權限
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @OwnerRoleId
        AND PermissionId = @ViewId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@OwnerRoleId, @ViewId);
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @OwnerRoleId
        AND PermissionId = @ManageId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@OwnerRoleId, @ManageId);
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @OwnerRoleId
        AND PermissionId = @PublishId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@OwnerRoleId, @PublishId);
-- 4. Admin：Manage + Publish
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @AdminRoleId
        AND PermissionId = @ViewId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@AdminRoleId, @ViewId);
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @AdminRoleId
        AND PermissionId = @ManageId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@AdminRoleId, @ManageId);
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @AdminRoleId
        AND PermissionId = @PublishId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@AdminRoleId, @PublishId);
-- 5. Member：僅 View
IF NOT EXISTS (
    SELECT 1
    FROM RolePermissions
    WHERE RoleId = @MemberRoleId
        AND PermissionId = @ViewId
)
INSERT INTO RolePermissions (RoleId, PermissionId)
VALUES (@MemberRoleId, @ViewId);
COMMIT TRANSACTION;
SELECT N'Merchant Case 預設角色與權限已建立。' AS Result;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;