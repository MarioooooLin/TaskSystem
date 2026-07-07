<#
.SYNOPSIS
    產生管理後台帳號的 BCrypt 雜湊，並輸出可直接執行的 SQL INSERT。

.EXAMPLE
    .\create-admin-seed.ps1 -Email "admin@ttm.com.tw" -Name "系統管理員" -Password "Admin@12345"
#>
param(
    [string]$Email = "admin@ttm.com.tw",
    [string]$Name = "系統管理員",
    [string]$Password = "Admin@12345"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir

# ── 1. Build Infrastructure（確保 BCrypt DLL 已編譯）───────────────
Write-Host "Building Infrastructure..." -ForegroundColor Cyan
dotnet build "$solutionDir/Infrastructure/Infrastructure.csproj" `
    --configuration Debug --no-restore --verbosity quiet

# ── 2. 載入 BCrypt.Net-Next DLL ────────────────────────────────────
$bcryptDll = "$solutionDir/Infrastructure/bin/Debug/net9.0/BCrypt.Net-Next.dll"

if (-not (Test-Path $bcryptDll)) {
    Write-Error "找不到 BCrypt.Net-Next.dll：$bcryptDll`n請先執行 dotnet build"
    exit 1
}

Add-Type -Path $bcryptDll

# ── 3. 產生 BCrypt Hash（WorkFactor = 12）──────────────────────────
Write-Host "Hashing password..." -ForegroundColor Cyan
$hash = [BCrypt.Net.BCrypt]::HashPassword($Password, 12)

# ── 4. 輸出 SQL ────────────────────────────────────────────────────
$sql = @"
-- ================================================================
-- Admin 帳號 Seed
-- Email    : $Email
-- Password : $Password（明文，勿存版控）
-- ================================================================

-- 確認帳號不重複再插入
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = '$Email')
BEGIN
    INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status)
    VALUES (1, N'$Name', '$Email', '$hash', 1);

    PRINT '管理員帳號建立完成：$Email';
END
ELSE
BEGIN
    PRINT '帳號已存在，略過插入：$Email';
END
"@

Write-Host "`n===== 以下 SQL 請貼到 SSMS 執行 =====" -ForegroundColor Yellow
Write-Host $sql
Write-Host "======================================" -ForegroundColor Yellow

# ── 5. 同時輸出到檔案 ──────────────────────────────────────────────
$outFile = Join-Path $scriptDir "seed-admin.sql"
$sql | Out-File -FilePath $outFile -Encoding UTF8
Write-Host "`nSQL 已儲存至：$outFile" -ForegroundColor Green
