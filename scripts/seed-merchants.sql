-- ================================================================
-- SEED DATA — 業者測試資料
-- 執行前提：schema.sql 已套用、Users 表已有管理員帳號
-- ================================================================

-- ── 1. 業者 Owner 帳號（AccountType=2） ──────────────────────────
-- PasswordHash = NULL（業者透過業者端登入，Admin 不需要以業者身分登入測試）
INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
VALUES
(2, N'王小美', N'owner@mountainsea.com.tw', NULL, 1),
(2, N'陳大明', N'owner@taipeihotel.com.tw', NULL, 1),
(2, N'林阿花', N'owner@hualien-bb.com.tw',  NULL, 1),
(2, N'李文山', N'owner@tainan-old.com.tw',  NULL, 1),
(2, N'張美惠', N'owner@kenting-resort.com.tw', NULL, 1);

-- ── 2. Merchants 記錄 ────────────────────────────────────────────
DECLARE @u1 BIGINT, @u2 BIGINT, @u3 BIGINT, @u4 BIGINT, @u5 BIGINT;

SELECT @u1 = Id FROM Users WHERE Email = 'owner@mountainsea.com.tw';
SELECT @u2 = Id FROM Users WHERE Email = 'owner@taipeihotel.com.tw';
SELECT @u3 = Id FROM Users WHERE Email = 'owner@hualien-bb.com.tw';
SELECT @u4 = Id FROM Users WHERE Email = 'owner@tainan-old.com.tw';
SELECT @u5 = Id FROM Users WHERE Email = 'owner@kenting-resort.com.tw';

-- VerificationStatus: 1=Pending 2=Approved 3=Rejected 4=Suspended
INSERT INTO Merchants
    (UserId, CompanyName, EnglishName, TaxId, IndustryType,
     ContactName, Phone, Fax, CompanyEmail, Website, Address,
     EstablishedDate, VerificationStatus, VerifiedAt)
VALUES
(@u1, N'山海旅宿股份有限公司', N'Mountain Sea Stay Co.', N'12345678', N'旅宿',
 N'王小美', N'02-2720-8889', NULL,
 N'service@mountainsea.com.tw', N'https://www.mountainsea.com.tw',
 N'台北市信義區信義路五段7號', '2018-05-20', 2, GETUTCDATE()),

(@u2, N'台北精品旅館有限公司', N'Taipei Boutique Hotel Ltd.', N'23456789', N'旅宿',
 N'陳大明', N'02-2345-6789', N'02-2345-6790',
 N'info@taipeihotel.com.tw', NULL,
 N'台北市大安區忠孝東路四段100號', '2015-03-10', 2, GETUTCDATE()),

(@u3, N'花蓮民宿集團', NULL, N'34567890', N'民宿',
 N'林阿花', N'038-123-456', NULL,
 N'hello@hualien-bb.com.tw', NULL,
 N'花蓮縣花蓮市中正路1號', '2020-08-15', 2, GETUTCDATE()),

(@u4, N'台南老宅民宿', NULL, N'45678901', N'民宿',
 N'李文山', N'06-222-3333', NULL, NULL, NULL,
 N'台南市中西區府前路一段200號', '2019-11-01', 1, NULL),

(@u5, N'墾丁海景渡假村有限公司', N'Kenting Ocean View Resort', N'56789012', N'旅宿',
 N'張美惠', N'08-888-9999', NULL,
 N'resort@kenting.com.tw', N'https://www.kenting-resort.com.tw',
 N'屏東縣恆春鎮墾丁路100號', '2016-07-04', 4, GETUTCDATE());

-- ── 3. MerchantWallets ───────────────────────────────────────────
DECLARE @m1 BIGINT, @m2 BIGINT, @m3 BIGINT, @m4 BIGINT, @m5 BIGINT;

SELECT @m1 = Id FROM Merchants WHERE UserId = @u1;
SELECT @m2 = Id FROM Merchants WHERE UserId = @u2;
SELECT @m3 = Id FROM Merchants WHERE UserId = @u3;
SELECT @m4 = Id FROM Merchants WHERE UserId = @u4;
SELECT @m5 = Id FROM Merchants WHERE UserId = @u5;

INSERT INTO MerchantWallets (MerchantId, AvailableAmount, FrozenAmount, TotalDepositedAmount)
VALUES
(@m1, 50000.00, 12000.00, 200000.00),
(@m2, 30000.00,  5000.00, 100000.00),
(@m3, 15000.00,     0.00,  50000.00),
(@m4,     0.00,     0.00,      0.00),
(@m5, 80000.00, 25000.00, 500000.00);

SELECT N'Seed 完成：5 筆業者資料已插入' AS Result;
