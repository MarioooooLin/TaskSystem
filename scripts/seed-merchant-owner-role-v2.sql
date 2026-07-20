-- ================================================================
-- Seed: 為特定 Merchant 登入帳號賦予 Owner 角色與案件權限（v2）
-- 用途：Merchant 站台權限資料尚未建立時，快速賦權給測試帳號
-- 注意：執行後請重新登入 Merchant 站台，權限才會寫入 Cookie
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
-- 2. 建立 Merchant Scope 的 Owner 角色
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
DECLARE @OwnerRoleId BIGINT;
SELECT @OwnerRoleId = Id
FROM Roles
WHERE Name = N'Owner'
    AND Scope = 2;
-- 3. Owner 綁定所有 Merchant.Case 權限
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
-- 4. 將指定帳號的 MerchantMembers.RoleId 設為 Owner，並確保狀態為 Active
-- 請把 @Email 換成實際登入的 Merchant 帳號 Email
DECLARE @Email NVARCHAR(255) = 'test-merchant@example.com';
UPDATE mm
SET RoleId = @OwnerRoleId,
    Status = 1
FROM MerchantMembers mm
    JOIN Users u ON u.Id = mm.UserId
WHERE u.Email = @Email;
COMMIT TRANSACTION;
-- 顯示結果供確認
SELECT u.Email,
    r.Name AS RoleName,
    p.Code AS PermissionCode,
    p.Description
FROM Users u
    JOIN MerchantMembers mm ON mm.UserId = u.Id
    JOIN Roles r ON r.Id = mm.RoleId
    JOIN RolePermissions rp ON rp.RoleId = r.Id
    JOIN Permissions p ON p.Id = rp.PermissionId
WHERE u.Email = @Email;
END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
THROW;
END CATCH;