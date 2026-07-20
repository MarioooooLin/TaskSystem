````instructions
# GitHub Copilot Instructions

<!-- ================================================================
  固定區：所有專案通用，換專案時不需修改
================================================================ -->

## 回答流程

1. **分析**：先簡短分析需求，確認影響哪些層
2. **計畫**：修改前先列出要執行的步驟
3. **執行**：進行修改
4. **驗證**：修改後確認編譯無錯誤

---

## 核心原則

- **語言**：所有解釋與對話強制使用**繁體中文**
- **風格**：簡潔有力，直接提供解決方案
- **準確性**：禁止臆測，優先確保可執行性
- **安全優先**：主動避免 OWASP Top 10
- **不確定時**：主動提問，列出 2～3 個選項讓使用者決策，不自行假設
- **最小修改原則**：只改需求涉及的程式碼，不主動重構無關的部分
- **精簡回答**：避免重複說明已知資訊，不需要的前言與總結一律省略
- **套件**：優先使用已安裝的套件解決問題，新增套件前須說明原因
- **測試**：不主動產生測試程式碼，除非明確要求；需要時優先針對 Service 層寫單元測試（xUnit + Moq）
- **參考上下文**：回答前先查閱以下文件
    - `.github/BRIEFING.md` — 完整需求、技術選型、DB Schema
    - `.github/MEMORY.md` — 過去的對話紀錄、決策原因（**務必先查，避免走回頭路**）
    - `.github/CONTRIBUTING.md` — 架構規範與 API 設計規則

---

## MEMORY.md 更新機制（強制）

完成任何代碼變更後，依序執行：

1. `get_errors` 驗證無編譯錯誤
2. 回報變更內容與驗證結果
3. 等待使用者測試後回報測試通過
4. 使用者回報測試通過後，取得當前時間（Windows：`Get-Date -Format "yyyy-MM-dd HH:mm"`）
5. 更新 `MEMORY.md`，格式如下：
6. 回應使用者

### 記錄格式

```markdown
### [HH:MM] 變更摘要

**變更內容**

- 具體做了什麼、影響哪些檔案

**決策原因**

- 為什麼這樣做，有哪些替代方案被排除
```

### MEMORY.md 保留與清理規則

- `MEMORY.md` 定期清理超過 30 天的流水帳紀錄，避免記憶檔過度膨脹。
- 清理前需先保留仍有效的架構決策、PM 定案、待確認事項、資料庫規則、權限規則與重大風險說明；必要時先濃縮成摘要，再刪除原始舊紀錄。
- 清理後更新檔頭的「最後整理時間」，並在當日區塊新增一筆清理摘要，說明刪除範圍與保留重點。

---

## Commit Message 規範

```
feat(scope): 新增功能描述
fix(scope): 修正問題描述
refactor(scope): 重構說明
docs(scope): 文件更新
```

---

<!-- ================================================================
  專案區：每個新專案填寫，換專案時只改這裡
================================================================ -->

## 專案資訊

- **專案名稱**：TTM 旅圖任務系統（TaskSystem）
- **Solution 路徑**：`c:\旅圖\任務系統\TaskSystem.sln`
- **技術棧**：.NET 9 / ASP.NET Core MVC / MSSQL / Dapper
- **三個 MVC 站台**：`Admin`（後台）、`Merchant`（業者）、`Kol`（KOL）
- **外部設定檔**：三個站台共用方案根目錄的 `Account/TaskSystem.json`，並透過 `AddTaskSystemExternalConfiguration()` 以必要檔案載入；檔案缺少時不得略過或改成 optional
- **設定優先順序**：各站台的 `appsettings.json` 仍由 ASP.NET Core 載入，但 `TaskSystem.json` 後載入且同名設定以它為準；連線字串、Cookie、Serilog、平台參數等共用或機密設定不得重複寫入各站台的 `appsettings.json`
- **機密管理**：`Account/` 已由 `.gitignore` 排除；不得提交 `TaskSystem.json`，也不得將其中的密鑰、連線字串或憑證搬入版控檔案

---

## 當前開發階段：後端優先

**本專案目前處於「後端先行」階段，前端畫面尚未完成。**

### 規則（強制）

- **View 檔案（`.cshtml`）暫不建立**，Controller Action 統一以 `return View()` 佔位
- **ViewModel 必須完整定義**所有輸入輸出欄位（含 DataAnnotations 驗證）
- **Use Case Handler 必須完整實作**，包含業務邏輯與 Repository 呼叫
- **Repository 必須實作完整 Dapper SQL**，不可用 stub 或 TODO 替代
- Controller 加上正確的 `[Authorize]`、`[HttpGet]`、`[HttpPost]`、Rate Limit Attribute

### 每個功能的實作檔案清單

```
ViewModel（Admin/ViewModels/功能名稱/）
→ Command 或 Query（Application/功能名稱/）
→ Handler（Application/功能名稱/）
→ Repository 介面方法（如有新增）
→ Repository 具體 SQL（Infrastructure/Persistence/Repositories/）
→ Controller Action（Admin/Controllers/）
```

### HTML 轉 cshtml 規則（強制）

- 前端畫面若由既有 HTML 轉換為 `.cshtml`，排版、DOM 結構、class 命名與視覺樣式應以原 HTML 為主。
- 轉換時只加入 Razor 必要語法，例如 `@model`、`asp-action`、`asp-route-*`、條件顯示、迴圈與表單驗證訊息。
- 不得因後端資料綁定而任意重排版面、改動 CSS class、改變區塊順序或重新設計 UI。
- 若 HTML 與後端資料結構不一致，先保留原畫面結構，並標記 `待確認`。

---

## Admin 功能開發順序

| #   | 功能                    | Controller                     |
| --- | ----------------------- | ------------------------------ |
| 1   | 登入 / 登出             | `AccountController`            |
| 2   | 後台帳號管理            | `AdminAccountController`       |
| 3   | RBAC 角色與權限設定     | `RolePermissionController`     |
| 4   | 業者管理（列表 / 審核） | `MerchantManagementController` |
| 5   | KOL 管理（列表 / 審核） | `KolManagementController`      |
| 6   | 案件監控                | `CaseMonitorController`        |
| 7   | 爭議處理                | `DisputeController`            |
| 8   | 帳務管理                | `FinanceController`            |
| 9   | 系統參數設定            | `SystemSettingController`      |
| 10  | 營運總覽 Dashboard      | `DashboardController`          |

---

## 分批確認規則（強制）

- 每完成一個「檔案」或一個「邏輯段落」後，**停下來等使用者確認**再繼續
- 不可一次生成超過 1 個功能的全部檔案
- 每次完成後說明「下一步是什麼」，讓使用者決定是否繼續

---

## C# 程式碼慣例

### 命名規則

| 項目 | 慣例 | 範例 |
| --- | --- | --- |
| 非同步方法 | 加 `Async` 後綴 | `HandleAsync` |
| private 欄位 | 底線前綴 | `_caseRepo` |
| Command / Query | `sealed record` | `LoginCommand` |
| Handler | `sealed class` + Constructor Injection | `LoginHandler` |
| ViewModel | `XxxViewModel` | `LoginViewModel` |

### Controller Action 模板

```csharp
// GET：查詢頁
[HttpGet]
public async Task<IActionResult> Index()
{
    var query = new XxxQuery();
    var result = await _handler.HandleAsync(query);
    return View(result.Value);
}

// POST：寫入操作
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(XxxViewModel vm)
{
    if (!ModelState.IsValid) return View(vm);

    var cmd = new XxxCommand(vm.Field1, vm.Field2);
    var result = await _handler.HandleAsync(cmd);

    if (result.IsFailure)
    {
        ModelState.AddModelError("", result.Error.Message);
        return View(vm);
    }

    return RedirectToAction(nameof(Index));
}
```

### ErrorType → HTTP 對應

| ErrorType | HTTP | Controller 處理方式 |
| --- | --- | --- |
| `NotFound` | 404 | `return NotFound()` |
| `Forbidden` | 403 | `return Forbid()` |
| `Conflict` | 409 | `ModelState.AddModelError` |
| `Validation` | 400 | `ModelState.AddModelError` |
| `Problem` | 500 | `return StatusCode(500)` |

### Use Case Handler 結構

```csharp
// Command（寫入操作）
public sealed record XxxCommand(string Field1, int Field2);

// Query（唯讀查詢）
public sealed record XxxQuery(int Page = 1, int PageSize = 20);

// Handler
public sealed class XxxHandler(
    IXxxRepository xxxRepo,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(XxxCommand cmd, CancellationToken ct = default)
    {
        // 1. 權限驗證
        // 2. 資料查詢
        // 3. 業務規則
        // 4. 寫入（await using var uow = await unitOfWork.BeginAsync()）
        // 5. return Result.Success()
    }
}
```

### Repository Dapper 慣例

```csharp
// 查單筆（找不到回 null）
await conn.QueryFirstOrDefaultAsync<Entity>(sql, param, tx);

// 查多筆
await conn.QueryAsync<Entity>(sql, param, tx);

// 寫入並取得新 ID
await conn.ExecuteScalarAsync<long>(sql + " SELECT SCOPE_IDENTITY()", param, tx);

// 更新 / 刪除
await conn.ExecuteAsync(sql, param, tx);
```
````

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.
