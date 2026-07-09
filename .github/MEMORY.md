# MEMORY.md — TaskSystem 開發記錄

> 最後整理時間：2026-07-09 17:54

---

## 2026-07-09

### [17:54] 業者管理 HTMX 局部刷新與捲動修正

**變更內容**

- `Admin/Controllers/MerchantManagementController.cs`：
    - `Index`、`Detail`、`Update` GET Action 與 `ReturnDetailWithError` 依 `HX-Request` header 回傳 `PartialView` 或 `View`
- `Admin/Views/MerchantManagement/Index.cshtml`：
    - 「新增業者」、「清除篩選」、「查看詳情」、「查看案件」、「查看帳務」加上 `hx-get` / `hx-target="#main-content"` / `hx-push-url="true"`
    - 分頁「跳至」輸入框改為 `htmx.ajax`，不再整頁跳轉
- `Admin/Views/MerchantManagement/Detail.cshtml`：
    - CSS 由 `@section PageCss` 改為直接輸出 `<link>`
    - 「返回業者管理」、「編輯業者資料」加上 htmx 屬性
- `Admin/Views/MerchantManagement/Update.cshtml`：
    - CSS 由 `@section PageCss` 改為直接輸出 `<link>`
    - 「取消」加上 htmx 屬性
- `Admin/Views/Shared/_Layout.cshtml`：側邊欄「業者管理」連結加上 htmx 屬性
- `Admin/wwwroot/js/main.js`：新增 `htmx:afterSwap` 全域監聽，每次 htmx 局部刷新後 `window.scrollTo(0, 0)`

**決策原因**

- 對齊營運總覽與 KOL 審核頁的 HTMX 模式：Controller 回傳 `PartialView`、連結帶 htmx 屬性、CSS 直接輸出
- 頁面捲動容器是 `body` 而非 `#main-content`，所以 `hx-swap="scroll:top"` 無效，改在 `main.js` 統一處理
- 全域 `htmx:afterSwap` 會套用到所有 htmx 請求，維持各頁面跳轉體驗一致

---

### [17:16] Dashboard / KOL 審核待辦 HTMX 局部刷新修正

**變更內容**

- `Admin/Views/Dashboard/Index.cshtml`：
    - 「查看全部KOL待審」連結加上 `hx-get`、`hx-target="#main-content"`、`hx-push-url="true"`
    - KOL 審核待辦表格「檢視明細」連結加上相同 HTMX 屬性
- `Admin/Controllers/KolManagementController.cs`：
    - `ReviewList` Action 依 `HX-Request` header 回傳 `PartialView` 或 `View`
    - `ReviewDetail` Action 依 `HX-Request` header 回傳 `PartialView` 或 `View`
- `Admin/Views/KolManagement/ReviewList.cshtml`：CSS 由 `@section PageCss` 改為直接輸出 `<link>`，避免 HTMX 局部載入時遺失樣式
- `Admin/Views/KolManagement/ReviewDetail.cshtml`：同上，CSS 改為直接輸出

**決策原因**

- HTMX 局部刷新必須同時滿足三個條件：觸發連結帶 `hx-get` + `hx-target`、Controller 回傳 `PartialView`、目標 View 的 CSS 直接輸出（非 `@section PageCss`）
- 最初只改了 `Detail` Action，但 Dashboard 連結指向 `ReviewDetail`，導致 HTMX 拿到完整 Layout，出現雙層 header 與搜尋列
- 測試確認：「查看全部KOL待審」與「檢視明細」均已正常局部刷新

---

## 2026-07-08

### [18:10] KOL 審核詳情頁實作（ReviewDetail）

**變更內容**

- `Application/Kols/DTOs/KolDetailDto.cs`：新增 `SubmittedAt` 屬性（送審時間近似值）
- `Application/Kols/Queries/GetKolDetailHandler.cs`：補 `SubmittedAt = baseDto.UpdatedAt` 映射
- `Admin/Controllers/KolManagementController.cs`：新增三個 Action
    - `ReviewDetail` GET：重用 `GetKolDetailHandler`，回傳 `ReviewDetail.cshtml`
    - `ReviewApprove` POST：呼叫 `ApproveKolHandler`，成功後導回 `ReviewList`
    - `ReviewReject` POST：呼叫 `RejectKolHandler`，成功後導回 `ReviewList`
- `Admin/Views/KolManagement/ReviewDetail.cshtml`（新建）：對齊 `refs/kol-review.html`
    - 摘要卡（頭像字符、名稱、審核狀態 badge、KOL類型/平台/粉絲/收款/任務統計）
    - 基本資料（個人資料 + KOL定位）
    - 社群頻道 table + 待確認帳號提醒（VerificationStatus == 3）
    - 收款資料（帳號遮蔽 + 隱私提示）
    - KOL審核（送審時間、退回原因、退回修改+審核通過按鈕並排）
- `Admin/Views/KolManagement/ReviewList.cshtml`：「進入審核」連結由 `Detail` 改為 `ReviewDetail`

**決策原因**

- `KolDetailDto` 已含所有所需欄位，直接重用，不另建 ReviewDetailDto
- 兩顆按鈕各自綁定獨立 `<form>`，透過 HTML `form="form-id"` 屬性並排顯示
- ReviewApprove / ReviewReject 完成後導回 `ReviewList`（非 Detail），符合審核流程
- `SubmittedAt` 以 `KolDetailBaseDto.UpdatedAt` 近似，待審核 KOL 最後更新時間即為送審時間

---

### [17:25] 審核新進 KOL 列表頁串接後端 + SQL 修正

**變更內容**

- `Application/Kols/DTOs/KolReviewSummaryDto.cs`（新建）：5 個 KPI 欄位（PendingCount / ResubmitCount / ReturnedCount / TodayNewCount / OverdueCount）
- `Application/Kols/Queries/GetKolReviewSummaryHandler.cs`（新建）：查詢審核摘要統計
- `Application/Kols/DTOs/KolReviewListItemDto.cs`：新增 `IsResubmit` 欄位，用於區分「待審核」vs「重送審核」
- `Application/Kols/Queries/GetKolReviewListQuery.cs`：新增 `Category`、`Platform`、`SubmittedDate` 篩選欄位
- `Application/Kols/Queries/GetKolReviewListHandler.cs`：傳遞新查詢參數
- `Application/Abstractions/Repositories/IKolRepository.cs`：新增 `GetReviewSummaryAsync`，更新 `GetReviewListAsync` 簽章
- `Infrastructure/Persistence/Repositories/KolRepository.cs`：
    - 實作 `GetReviewSummaryAsync`
    - `GetReviewListAsync` SQL 加入 `IsResubmit`、`Category`、`Platform`、`SubmittedDate` 篩選
- `Application/DependencyInjection.cs`：註冊 `GetKolReviewSummaryHandler`
- `Admin/ViewModels/Kol/KolReviewIndexViewModel.cs`（新建）：包含 List + Summary + Query
- `Admin/ViewModels/Kol/KolReviewListQueryViewModel.cs`（新建）：篩選條件 ViewModel
- `Admin/Controllers/KolManagementController.cs`：`ReviewList` Action 改用 `KolReviewIndexViewModel`，注入 `GetKolReviewSummaryHandler`
- `Admin/Views/KolManagement/ReviewList.cshtml`：完整重寫，對齊 `refs/kol-new.html` 設計稿
    - 5 欄 KPI 卡（超過3日未審標紅色）
    - 狀態篩選 chip bar（待審核/重送審核/已退回待補/已通過）
    - 進階篩選（KOL類型、主要平台、資料完整度、社群資料、送審日期）
    - 資料表（KOL/類型、主要平台 badge、追蹤總數、進度條完整度、審核狀態 pill、送審時間、進入審核按鈕）
    - 分頁元件

**SQL 修正（GetReviewSummaryAsync）**

- **錯誤原因**：在 `SUM()` 彙總函式內使用關聯子查詢（`EXISTS` 引用外層 `kp.Id`），SQL Server 不支援此語法
- **錯誤訊息**：`不得在包含彙總或子查詢的運算式上執行彙總函式`
- **修法**：改用 `LEFT JOIN (SELECT DISTINCT KolId FROM KolReviewEvents WHERE ActionType = 2) kre ON kre.KolId = kp.Id`
    - `PendingCount`：`kre.KolId IS NULL`（無重送事件 = 純待審核）
    - `ResubmitCount`：`kre.KolId IS NOT NULL`（有重送事件 = 重送審核）

**決策原因**

- `IsResubmit` 欄位由 SQL 端透過 `LEFT JOIN KolReviewEvents` 判斷，不額外查詢
- 審核狀態 pill 顏色：重送審核用深藍、已退回待補用紅色、待審核用灰色

**⚠️ 下一步（新對話繼續）**

- `ReviewDetail` Action 與 `ReviewDetail.cshtml` 尚未實作（對應 `refs/kol-review.html`）
- 現有「進入審核」按鈕暫連至 `/KolManagement/Detail/{id}`（KOL 管理詳情頁），待 ReviewDetail 實作後修正連結

---

## 2026-07-08

### [16:30] KOL 詳情頁後端全接通（ADM-006）+ ActivityLog 補實作

**變更內容**

- `Admin/Views/KolManagement/Detail.cshtml`：完整重寫，`@model KolDetailDto`，對齊 `kol-detail.html` 切版
    - 摘要卡（名稱、狀態 badge、類型 chips、平台、粉絲數、收款狀態、任務/異議統計）
    - 基本資料（個人資料 + KOL 定位：條件、自介、可合作條件）
    - 社群頻道 table（平台 icon、帳號、粉絲數、資料來源、更新日期）
    - 收款資料（帳號遮蔽，附隱私提示）
    - KOL 管理：依 VerificationStatus 條件顯示（待審核→通過+退回；啟用中→停權；停權→解除）
    - 任務追蹤 table（最新 10 筆，連結至案件詳情）
    - 收益摘要（待月結/待匯款/已匯款/可請款餘額）
    - 操作紀錄（ActivityLogs 最新 10 筆）
- `IActivityLogRepository.cs`（新建）：`WriteAsync` 介面
- `ActivityLogRepository.cs`（新建）：Dapper INSERT INTO ActivityLogs
- `Infrastructure/DependencyInjection.cs`：註冊 `IActivityLogRepository`
- `ApproveKolHandler` / `RejectKolHandler` / `SuspendKolHandler` / `UnsuspendKolHandler`：Commit 前寫入 ActivityLogs

**Repository SQL 修正（schema 對齊）**

| 位置                                      | 錯誤                                                 | 修正                           |
| ----------------------------------------- | ---------------------------------------------------- | ------------------------------ |
| `KolStatsRepository.GetStatsByKolIdAsync` | `Cases.KolId` 不存在                                 | 改透過 `Tasks` JOIN `Disputes` |
| `KolStatsRepository.GetStatsByKolIdAsync` | `CompletedTaskCount Status = 5`（RevisionRequested） | 改為 `= 6`（Completed）        |
| `KolStatsRepository.GetStatsByKolIdAsync` | `PendingReviewCount IN (3, 4)`                       | 改為只取 `= 4`（UnderReview）  |
| `KolStatsRepository.GetRecentTasksAsync`  | `Tasks.CreatedAt` 不存在                             | 改為 `Cases.CreatedAt`         |

**決策原因**

- `Tasks` 表無 `CreatedAt`，以 `Cases.CreatedAt`（案件建立時間）代替排序
- ActivityLogs `TargetType = 'KolProfiles'`，與 KolStatsRepository 讀取條件對應
- 四個 KOL 操作 Handler 皆在同一 transaction 內寫入 log，保證原子性
- ViewModel 兩處 HTML 與 DTO 不一致：「放棄任務次數」→ 驗收中任務（PendingReviewCount）；「導購分潤累計」→ 可請款餘額（AvailableAmount）

---

## 2026-07-07

### [19:16] KOL 管理首頁實作（ADM-005）+ htmx 局部刷新

**變更內容**

- 新建 `Application/Kols/DTOs/KolSummaryDto.cs`（6 個 KPI 欄位：TotalCount / ActiveCount / PendingCount / RejectedCount / SuspendedCount / AbnormalCount）
- 新建 `Admin/ViewModels/Kol/KolIndexViewModel.cs`（List + Summary + Query 包裝）
- 新建 `Application/Kols/Queries/GetKolSummaryHandler.cs`
- `Application/Kols/DTOs/KolListItemDto.cs`：新增 `CompletedTaskCount`
- `Admin/ViewModels/Kol/KolListQueryViewModel.cs`：新增 `DateFrom`、`DateTo`
- `Application/Kols/Queries/GetKolListQuery.cs`：新增 `DateFrom`、`DateTo`
- `Application/Kols/Queries/GetKolListHandler.cs`：傳遞新參數
- `Application/Abstractions/Repositories/IKolRepository.cs`：新增 `GetSummaryAsync`、更新 `GetListAsync` 簽章
- `Infrastructure/Persistence/Repositories/KolRepository.cs`：
    - 修正 SQL（`TaskCount` / `DisputeCount` 原本錯誤地查 `Cases.KolId`，改為正確的 `Tasks.KolId`）
    - 修正 `kba.VerificationStatus` → `kba.Status`（`KolBankAccounts` 無 `VerificationStatus` 欄位）
    - 實作 `GetSummaryAsync`；加入日期篩選
- `Application/DependencyInjection.cs`：註冊 `GetKolSummaryHandler`
- `Admin/Controllers/KolManagementController.cs`：注入 summaryHandler，Index 改回傳 `KolIndexViewModel`
- `Admin/Views/KolManagement/Index.cshtml`：完整重寫，對齊切版，含 KPI 卡、chip 篩選、資料表、分頁

**htmx 局部刷新（KOL + 業者管理頁）**

- `Admin/Views/Shared/_Layout.cshtml`：加入 htmx CDN（`unpkg.com/htmx.org@2.0.4`）
- `KolManagement/Index.cshtml`：form 加 `hx-get / hx-target / hx-select / hx-swap / hx-push-url`；table+pagination 包在 `<div id="kol-results">`；分頁連結加 `hx-boost`
- `MerchantManagement/Index.cshtml`：同上，id 為 `merchant-results`

**決策原因**

- htmx `hx-select` 方案：伺服器回傳完整 HTML，htmx 只取指定 element 替換，零後端改動
- `hx-push-url="true"` 同步 URL，瀏覽器上一頁可正常使用
- 分頁連結使用 `hx-boost="true"` 攔截點擊，繼承父層 target/select

**⚠️ 待解決**

- htmx 搜尋功能有異常（明日繼續排查），目前未確認確切錯誤訊息
- 可能原因：`hx-get` 與 chip `type="submit"` 按鈕的值傳遞方式需確認；或 `hx-select` 抽取邏輯問題

**決策原因**

- `KolBankAccounts.Status` 不是 `VerificationStatus`，是 1=Pending/2=Verified/3=Rejected
- 原始 SQL 用 `Cases.KolId` 是錯的，Cases 沒有 KolId；正確路徑是 Tasks 表
- `CompletedTaskCount` 使用 `Tasks.Status = 6`（Completed）

---

## 2026-07-07

### [18:24] 調整 MEMORY.md 更新時機

**變更內容**

- 更新 `.github/copilot-instructions.md`
- 將 `MEMORY.md 更新機制` 改為：代碼變更完成並驗證後，先回報變更內容與驗證結果，等待使用者確認
- 使用者確認後，才取得當前時間並更新 `.github/MEMORY.md`

**決策原因**

- 使用者希望 MEMORY 紀錄不要在編譯無誤後立即寫入，而是等使用者確認後再記錄
- 此規則可避免尚未被使用者接受的中間變更提前進入專案記憶

### [18:07] 業者管理模組完成確認

**完成狀態**

| 頁面                                    | 對應 Template            | 後端接通           | 切版對齊 |
| --------------------------------------- | ------------------------ | ------------------ | -------- |
| 業者管理首頁 `MerchantManagement/Index` | business-management.html | ✅                 | ✅       |
| 業者詳情頁 `MerchantManagement/Detail`  | business-detail.html     | ✅                 | ✅       |
| 業者編輯頁 `MerchantManagement/Update`  | business-edit.html       | ✅（Handler 完整） | 靜態佔位 |

**其他待補頁面（靜態佔位，後端尚未接通）**

| 頁面                              | 優先順序 |
| --------------------------------- | -------- |
| `Dashboard/Index`                 | 高       |
| `CaseMonitor/Index` / `Detail`    | 高       |
| `AdminAccount/Index` / `Create`   | 中       |
| `RolePermission/Index` / `Detail` | 中       |
| `Finance/Index` / `Transactions`  | 中       |
| `SystemSetting/Index`             | 低       |
| `Account/ForgotPassword`          | 低       |

---

### [18:07] 業者管理首頁對齊切版（完整修復）

**變更內容**

- `Application/Merchants/DTOs/MerchantListItemDto.cs`：新增 `IndustryType`、`CreditAmount` 欄位
- `Application/Merchants/DTOs/MerchantSummaryDto.cs`：新建（TotalCount / ActiveCount / SuspendedCount）
- `Admin/ViewModels/Merchant/MerchantListQueryViewModel.cs`：新增 `IndustryType`、`DateFrom`、`HasCredit` 篩選參數
- `Admin/ViewModels/Merchant/MerchantIndexViewModel.cs`：新建，包裝 List + Summary + Query
- `Application/Merchants/Queries/GetMerchantListQuery.cs`：新增篩選參數
- `Application/Merchants/Queries/GetMerchantSummaryHandler.cs`：新建，查詢全域 KPI
- `Application/Abstractions/Repositories/IMerchantRepository.cs`：新增 `GetSummaryAsync`，更新 `GetListAsync` 簽章
- `Infrastructure/Persistence/Repositories/MerchantRepository.cs`：新增 `GetSummaryAsync` SQL；更新 `GetListAsync`（加 IndustryType / CreditAmount / DateFrom / HasCredit WHERE 條件）
- `Application/DependencyInjection.cs`：註冊 `GetMerchantSummaryHandler`
- `Admin/Controllers/MerchantManagementController.cs`：注入 summaryHandler，Index action 改回傳 `MerchantIndexViewModel`
- `Admin/Views/MerchantManagement/Index.cshtml`：完全對齊 HTML template

**變更要點**

- KPI 卡片改為「全部業者 / 啟用中 / 停用中」（全域計數，非篩選後數值）
- 篩選列從 1 列擴為 2 列（新增 客戶類型〔無DB欄位，標記待確認/disabled〕、建立日期、行業類型、是否有折扣金）
- 表格欄位：業者名稱 / 客戶類型 / 行業類型 / 統編 / 主要聯絡人（含 email） / 狀態 / 案件 / 錢包餘額 / 折扣金 / 建立日期 / 操作（3 按鈕）
- 狀態樣式改為 `bm-status bm-status--active/inactive/pending`
- 分頁改為「顯示第 X 至 Y 筆業者，共計 Z 筆資料」格式，加入跳頁 input

**決策原因**

- 客戶類型（公司/個人外包）DB 無對應欄位，暫用 disabled select 佔位並標記「待確認」
- 折扣金來自 `MerchantCreditWallets.AvailableAmount`（LEFT JOIN）
- 全域 KPI 由獨立 `GetMerchantSummaryHandler` 提供，不影響篩選邏輯

### [17:42] 更新 HTML 轉 cshtml 切版規則

**變更內容**

- 更新 `.github/copilot-instructions.md`
- 新增「HTML 轉 cshtml 規則（強制）」段落
- 規定由既有 HTML 轉換為 `.cshtml` 時，以原 HTML 的排版、DOM 結構、class 命名與視覺樣式為主
- 規定轉換時只加入必要 Razor 語法，不因資料綁定任意重排版面或重新設計 UI

**決策原因**

- 使用者希望專案前端畫面從 HTML 轉為 `.cshtml` 時，以 HTML 原始格式排版為主
- 此規則屬 AI/助理執行時的實作指令，放在 `.github/copilot-instructions.md` 最能直接約束後續轉檔行為

### [15:30] 業者詳情頁對齊切版（折扣金、案件統計、操作紀錄）

**變更內容**

- 新增 `Domain.Entities.MerchantCreditWallet`
- 新增 DTO：`MerchantCreditWalletSummaryDto`、`MerchantCreditTransactionDto`
- 更新 `MerchantStatsDto`：TaskCount → InProgressCount + CompletedCount
- 更新 `MerchantActivityLogDto`：加入 TargetType 欄位
- 更新 `MerchantDetailDto`：加入 CreditWallet、RecentCreditGrants、RecentCreditUsages
- 新增 `IMerchantCreditWalletRepository` + `MerchantCreditWalletRepository`（GetSummary/GetRecentGrants/GetRecentUsages/Upsert/InsertTransaction）
- 新增 `AdjustMerchantCreditCommand` + `AdjustMerchantCreditHandler`（加值/扣回，含餘額驗證）
- 新增 `AdjustCreditViewModel`
- 更新 `GetMerchantDetailHandler`：加入折扣金查詢
- 更新 `MerchantStatsRepository` SQL：案件狀態分項統計 + ActivityLogs 加 TargetType
- 更新 `MerchantManagementController`：加入 AdjustCredit POST action
- 更新 `Detail.cshtml`：代理登入(disabled)、折扣金Summary、案件統計分項、完整折扣金區塊（表單+紀錄表）、操作紀錄加欄位與色彩tag
- 更新 DI 兩處：Application + Infrastructure

**決策原因**

- 當前錢包總額 = AvailableAmount + FrozenAmount（非 TotalDepositedAmount）
- 折扣金加值/扣回屬高風險操作，需 confirm 對話框
- 操作類型 tag 顏色由 Action 字串前綴動態推導

**變更內容**

- `MerchantMemberRepository`：`u.DisplayName` → `u.Name`
- `MerchantStatsRepository`：ActivityLogs JOIN `KolProfiles` 改為 JOIN `Users`，`DisplayName` → `Name`
- `KolRepository`：`adm.DisplayName` (admin 查 Users) → `adm.Name`
- `KolStatsRepository`：ActivityLogs JOIN `KolProfiles` 改為 JOIN `Users`，`DisplayName` → `Name`

**決策原因**

- `Users` 資料表只有 `Name` 欄位，`DisplayName` 只存在於 `KolProfiles`
- ActivityLogs 的 ActorUserId 對應的是 `Users.Id`，應 JOIN `Users` 而非 `KolProfiles`

**變更內容**

- 新增 `Infrastructure/Persistence/Dapper/TypeHandlers/DateOnlyTypeHandler.cs`
- 在 `Infrastructure/DependencyInjection.cs` 最前面加上 `SqlMapper.AddTypeHandler(DateOnlyTypeHandler.Instance)`
- 修正業者詳情頁 `EstablishedDate` 欄位 `DataException: Error parsing column 11 (EstablishedDate=... DateTime)` 錯誤

**決策原因**

- SQL Server `date` 型別透過 Dapper 讀取時回傳 `DateTime`，C# 的 `DateOnly` 需要手動 TypeHandler 才能轉換
- 採用 Singleton Instance 模式，全域只需註冊一次

### [13:20] 業者狀態收斂為 Approved / Suspended

**決策與實作**

- 業者不走審核流程，`Merchants.VerificationStatus` 第一版只使用 `Approved = 2` 與 `Suspended = 4`。
- `Domain.Entities.Merchant` 預設狀態改為 `VerificationStatus.Approved`。
- `schema.sql` 中 `Merchants.VerificationStatus` 預設值改為 `2`，CHECK constraint 收斂為 `(2, 4)`。
- 修正 `SuspendUsersByMerchantAsync`：停用業者底下帳號時，`Users.Status` 應設為 `2 = Suspended`，不是 `3 = Deleted`。
- `UnsuspendMerchantHandler` 補上整體復權：業者恢復 Approved 後，同 transaction 將該業者 Active 成員關聯底下的 Suspended 使用者恢復 Active。

### [12:55] ADM-015 KOL 審核流程定案

**決策**

- `ADM-015` 不擴充 `VerificationStatus` 來表示「重送審核」等流程事件。
- 新增 `KolReviewEvents` 作為 KOL 資料審核事件表，記錄 Submitted、Resubmitted、Approved、Returned。
- `KolProfiles.VerificationStatus = Pending` 代表目前等待管理者審核；若最新送審/重送事件發生在退回事件之後，ADM-015 顯示「重送審核」。
- `KolProfiles.VerificationStatus = Rejected` 定義為「已退回待補」，不是永久拒絕。

**已更新**

- `schema.sql`：新增 `KolReviewEvents` 與查詢索引。
- `.github/CONTRIBUTING.md`：移除 ADM-015 流程狀態待確認項，補上定案規則。
- `.github/specs/admin-pages.md`：ADM-015/ADM-016 改用 `KolReviewEvents` 推導與記錄審核流程。
- `.github/specs/kol-pages.md`：KOL 送審與重送審核需寫入 `KolReviewEvents`。

### [12:45] 待確認事項更新：LINE Email 補填與 MER-HOME 案件狀態

**決策內容**

- LINE Login 若未提供 Email，KOL 首次登入需強制補填 Email 後才能建立或完成 `Users` 帳號；`Users.Email` 維持必填且全系統唯一。
- `MER-HOME` 不存在待審核案件狀態；若 Figma 或靜態切版出現待審核案件卡片，視為多出的示意狀態，不新增 `CaseStatus.PendingReview`。

**ADM-015 建議方向**

- 不建議擴充 `VerificationStatus` 來塞「重送審核」這種流程事件。
- 建議新增 KOL 審核流程紀錄表或等價 audit table，以最新審核事件推導 ADM-015 顯示狀態。
- `VerificationStatus.Pending` 只代表目前等待管理者審核；若最新送審事件發生在退回後，前端顯示「重送審核」。
- `VerificationStatus.Rejected` 代表「已退回待補」，不是永久拒絕。

### [12:35] ADM-011 帳務監控：預設期間與目前可記錄收支來源

**決策內容**

- `ADM-011` 預設統計期間為當月。
- 目前 schema 可記錄資金流入、案件鎖款/結算/釋放、折扣金折抵、KOL 收益、KOL 提領/月結與人工調整。
- 業者儲值屬資金流入，不直接等同平台營運收入；ADM-011 需區分「資金流入/流出」與「營運收入/支出」。

**第一版建議公式**

- `平台總收入 = 已完成案件結算收入 + 平台服務費 + 固定開案費 + 平台導購抽成 + 收入型人工調整 - 折扣金折抵開案費`
- `平台總支出 = KOL 淨付金額 + 支出型人工調整 + 退款/釋放調整`
- `平台毛利 = 平台總收入 - 平台總支出`
- 導購訂單收入、同步狀態與預估分潤仍需依後續 `ReferralOrders` 或等價資料表補齊。

### [12:25] PM 決策：ADM-008 批次操作第一版移除

**決策內容**

- `ADM-008` 案件詳情與進度頁不提供批次操作。
- 第一版不建立批次 Command，不提供多選處理。
- 頁面僅保留單筆查看、單筆爭議處理、附件下載等操作。
- 未來若恢復批次操作，需重新確認可作用狀態、操作範圍、權限與稽核要求。

**文件更新**

- `.github/CONTRIBUTING.md`：移除「ADM-008 批次操作可執行哪些動作、可作用於哪些任務狀態」待確認項，新增移除規則。
- `.github/specs/admin-pages.md`：ADM-008 批次操作改為第一版移除。

### [12:20] PM 決策：所有匯出功能第一版暫緩

**決策內容**

- Admin / Merchant / KOL 所有匯出功能第一版暫緩實作。
- Figma 若有匯出按鈕，可在 UI 保留停用/隱藏占位。
- 後端第一版不建立匯出 Command，不產生 CSV / Excel / PDF。
- 匯出欄位、格式、權限與稽核紀錄不再列為當前待確認問題；未來 PM 重新啟用匯出時再重新定義。

**文件更新**

- `.github/CONTRIBUTING.md`：新增「所有匯出功能第一版暫緩」共通規則，移除匯出欄位相關待確認事項。
- `.github/specs/admin-pages.md`：ADM-005、ADM-015、ADM-008、ADM-011 的匯出動作改為暫緩。
- `.github/specs/merchant-pages.md`：MER-014、MER-016 與業者角色權限中的匯出/下載報表改為暫緩。

### [12:10] 待確認事項更新：註冊欄位、業者復權、折扣金、多語系

**決策內容**

- 帳號註冊的登入憑證欄位，業者端與管理者端第一版皆為 Email + 密碼；公司資料、管理者姓名/部門/角色等屬延伸資料。
- 業者停權採整體停權：停用業者主檔後，該業者底下所有角色帳號皆不可登入或操作業者端。
- 業者復權第一版採整體復權：恢復業者主檔，並恢復因該業者停權而停用的成員帳號；若未來需保留成員個別停用狀態，再補停權前狀態快照或明確操作紀錄。
- 折扣金只有業者有，第一版只能折抵案件開案費，不可折抵 KOL 酬勞、導購分潤、儲值或其他款項。
- 多語系第一版只做 UI 介面文案多語系，不做使用者輸入內容多語系；固定文案使用 `.resx` 或等價資源檔，使用者輸入內容以原文保存與顯示。

**文件更新**

- `.github/CONTRIBUTING.md`：移除「帳號註冊所需欄位」、「業者復權規則」、「折扣金折抵規則」三個待確認項。
- `.github/specs/admin-pages.md`：更新 ADM-003 業者停權/復權與折扣金規則。
- `.github/specs/merchant-pages.md`：更新 MER-019 註冊欄位語意。
- `schema.sql`：更新 `MerchantCreditWallets` / `MerchantCreditTransactions` 註解。

### [11:50] Admin 登入流程測試通過

**變更內容**

- `Infrastructure/DependencyInjection.cs`：補全所有 Repository `AddScoped` 對應（9 個新 Repo）
- `Account/TaskSystem.json`：修正連線字串 Server IP 尾端多餘的句點（`192.168.0.144.` → `192.168.0.144`）
- `Admin/Controllers/HomeController.cs`：加上 `[Authorize]`，未登入自動導向 `/Account/Login`
- `Admin/Program.cs`：Cookie `SecurePolicy` 改為開發環境用 `SameAsRequest`、正式環境用 `Always`，解決 HTTP 開發時 Cookie 不被瀏覽器傳送的問題
- `Admin/wwwroot/css/login.css`：新增 `.alert-error` 樣式，讓登入失敗錯誤訊息可見
- `Admin/Controllers/DashboardController.cs`：新增佔位 Controller（登入後跳轉目標）

**決策原因**

- 登入失敗時錯誤訊息因無 CSS 樣式而隱形，導致誤以為「閃爍後回到登入頁」
- `CookieSecurePolicy.Always` 在 HTTP 開發環境下 Cookie 被瀏覽器拒絕傳送，造成登入成功卻持續被導回登入頁

### [11:04] 修復 DI 啟動錯誤：補建全部缺少的 Repository 實作

**變更內容**

- 新增 `Infrastructure/Persistence/Repositories/KolRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/KolSocialAccountRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/KolBankAccountRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/KolStatsRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/KolEarningRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/MerchantContactRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/MerchantStatsRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/MerchantMemberRepository.cs`
- 新增 `Infrastructure/Persistence/Repositories/MerchantWalletRepository.cs`
- 更新 `Infrastructure/DependencyInjection.cs`：補全所有 `AddScoped` 對應

**決策原因**

- 啟動 Admin 專案時 DI 容器驗證失敗，多個 Handler 注入的 Repository 介面未被實作/註冊
- 一次補齊所有 Abstractions/Repositories 下的介面，避免反覆修復啟動錯誤

---

## 2026-07-06

### [19:09] Admin Views 切版整合完成 — 全部 20 個 View 建立完畢

**變更內容**

- 建立本機 git repo（`git init`），初始 commit `ce50fde`，第二次 commit `766b6f7`
- `Admin/Controllers/MerchantManagementController.cs`：補 GET `Update(long id)` Action
- 修正 `KolManagement/ReviewList.cshtml`：CSS `kol-new.css` → `kol-review.css`
- 新建 **13 個** .cshtml（含今日完成全部）：

| View                               | CSS                                     |
| ---------------------------------- | --------------------------------------- |
| `MerchantManagement/Detail.cshtml` | business-detail.css                     |
| `MerchantManagement/Update.cshtml` | business-detail.css + business-edit.css |
| `Account/ForgotPassword.cshtml`    | login.css（\_LayoutLogin）              |
| `Dashboard/Index.cshtml`           | index.css                               |
| `CaseMonitor/Index.cshtml`         | 無                                      |
| `CaseMonitor/Detail.cshtml`        | cases-detail.css                        |
| `Finance/Index.cshtml`             | wallet.css                              |
| `Finance/Transactions.cshtml`      | transaction-history.css                 |
| `SystemSetting/Index.cshtml`       | settings.css                            |
| `AdminAccount/Index.cshtml`        | permission.css（額外新增）              |
| `AdminAccount/Create.cshtml`       | add-user.css                            |
| `RolePermission/Index.cshtml`      | permission-management.css               |
| `RolePermission/Detail.cshtml`     | permission.css                          |

- 靜態佔位 View 資料欄位均顯示「— 待資料庫連線 —」
- 所有 `.cshtml` 靜態資源路徑確認使用 `~/` 前綴

**決策原因**

- 業者/KOL 端 Template（accepting/add-cases/company-info/publish/register/referral/kol-new 等）不屬於 Admin，跳過
- `AdminAccount/Index.cshtml` 為額外新增（原計畫只有 Create），使用 permission.html 的成員列表內容

**目前 Admin Views 完成狀態**

全部 20 個 View 均已完成，無待建項目。

---

### [15:00] 補齊 CONTRIBUTING §7 定案規則缺漏：KOL/業者停權同步 + Entity 欄位 + SocialPlatform Enum

**變更內容**

- `Domain/Enums/SocialPlatform.cs`：從 6 個值（舊值對應 CasePlatforms，錯誤）整個替換為 schema.sql 定義的 11 個值
    - X=1, Instagram=2, Facebook=3, YouTube=4, Blog=5, XiaoHongShu=6, TikTok=7, Douyin=8, Threads=9, Snapchat=10, WeChat=11
- `Domain/Entities/Role.cs`：補 `Description?` 和 `IsSystemReserved` 屬性（對應 schema.sql Roles 表欄位）
- `Domain/Entities/Permission.cs`：補 `RiskLevel short`（default=1，對應 schema.sql Permissions 表欄位）
- `Application/Abstractions/Repositories/IUserRepository.cs`：新增 `SuspendUsersByMerchantAsync(long merchantId, ...)` 介面方法
- `Application/Kols/Commands/SuspendKolHandler.cs`：注入 `IUserRepository`，停權後在同一 transaction 同步 `Users.Status = Suspended`
- `Application/Kols/Commands/UnsuspendKolHandler.cs`：注入 `IUserRepository`，解停後在同一 transaction 同步 `Users.Status = Active`
- `Application/Merchants/Commands/SuspendMerchantHandler.cs`：注入 `IUserRepository`，停用業者後在同一 transaction 呼叫 `SuspendUsersByMerchantAsync` 批次停用底下所有 Active 成員的 Users
- 整個 Solution 建置：**0 錯誤**

**決策原因**

- CONTRIBUTING §7 定案：「KOL 停權同步更新 `KolProfiles.VerificationStatus = Suspended` 與 `Users.Status = Suspended`」
- CONTRIBUTING §7 定案：「KOL 解除停權先預設回到 `KolProfiles.VerificationStatus = Approved` 與 `Users.Status = Active`」
- admin-pages.md ADM-003 定案：「停用範圍預設包含該 Merchant 底下所有 MerchantMembers 對應的 Users，避免業者被停用後仍可操作業者端」
- `SuspendUsersByMerchantAsync` 採單一 SQL 批次 UPDATE（不逐一 load Entity），避免 N+1 問題
- `SocialPlatform` 原 6 值對應 CasePlatforms 平台順序（錯誤），與 KolSocialAccounts 的 11 值完全不同；現改為 KolSocialAccounts 正確對應
- DTO / Entity 中 Platform 欄位皆用 `short` 而非 enum，更換後不需改其他檔案
- `UnsuspendMerchantHandler` 後續應依整體復權規則補實作：恢復業者主檔，並恢復因該業者停權而停用的成員帳號

**尚未補的（Infrastructure 層）**

- `UserRepository.SuspendUsersByMerchantAsync` SQL 實作（待 Infrastructure 層補齊）
- KolRepository、MerchantRepository 等所有其他 Repository SQL 仍未實作

---

## 2026-07-01

### [13:52] 規劃盤點：缺漏規則文件、LINE 整合、導購串接、時程重估

**變更內容**

本次為討論記錄，無程式碼變更。

---

#### 一、規則文件（rules_utf8.htm）現況

目前文件只涵蓋到 R04，以下章節**完全尚未撰寫**：

| 缺的章節         | 影響功能                                                     |
| ---------------- | ------------------------------------------------------------ |
| R05 帳務規則     | 業者鎖款時機與金額、KOL 收益解凍條件、平台手續費計算         |
| R06 導購分潤規則 | 折扣碼產生方式、訂單對應 KOL 邏輯、分潤計算公式、Cookie 天數 |
| R07 爭議處理規則 | 爭議發起條件、爭議期間金流處理、管理者介入後的結算流程       |
| R08 撥款規則     | 月結或隨時申請、最低提領金額、發票/收據上傳流程              |

**決策原因**

- 帳務、導購、爭議三塊若沒有規則文件，Infrastructure SQL 寫完會再改，建議先補文件再寫 SQL

---

#### 二、Schema 缺漏

| 缺的表                    | 說明                                                              |
| ------------------------- | ----------------------------------------------------------------- |
| `UserExternalLogins`      | LINE Login OAuth 綁定（已在 MEMORY 討論，尚未加入 schema.sql）    |
| `ReferralOrders`          | 導購訂單，從旅圖訂房系統 API 抓回後對應 KOL/Case                  |
| `NotificationPreferences` | 通用通知偏好；KOL 個人與業者公司層級皆使用此表，依 OwnerType 區分 |

---

#### 三、導購串接方向確認

- 導購訂單來源為**旅圖訂房系統**，透過 API 取得
- 導購佣金比例已確認：
    - 業者開案輸入的 `Cases.CommissionRate` 是總佣金比例。
    - 最低可輸入比例 = Admin 設定的「平台抽成比例」+「KOL 最低分潤比例」。
    - 平台固定取得「平台抽成比例」；扣除平台抽成後剩餘比例全數歸 KOL。
    - 若業者輸入超過最低比例，超出的部分也全數歸 KOL。
- 需確認以下問題後才能設計 `ReferralOrders` 表：
    1. 訂房系統訂單 ID 格式（影響 `ExternalOrderId` 型別）
    2. 追蹤方式：折扣碼 / 專屬連結 / 兩者都有
    3. 分潤觸發點：訂單成立 or 入住/完成後
    4. API 模式：push（對方主動通知）or pull（我方定時拉取）
- 時程表備註「訂房系統/鐘點系統?」尚未確認，**建議列為開會待決項**

---

#### 四、LINE 整合 — 目前完全未規劃

KOL 端預計在 LINE 官方帳號內以 LIFF（LINE 內建瀏覽器）方式運作，涉及：

| 功能                     | 狀態              | 影響                                                                  |
| ------------------------ | ----------------- | --------------------------------------------------------------------- |
| LINE Login（LIFF OAuth） | ⚠️ 討論中，待確認 | `UserExternalLogins` 表、`Users.Email` nullable                       |
| LINE Messaging API 推播  | ❌ 完全未規劃     | 需要儲存 KOL LINE userId（非 LineContactId 聯絡欄位）供伺服器主動推播 |
| 通知偏好設定             | ✅ 已定案         | `NotificationPreferences` 通用表，對應 KOL-005 / MER-012 頁面         |
| LIFF App 設定            | ❌ 完全未規劃     | LINE Developer Console 設定、LIFF ID 管理                             |

**注意**：`KolProfiles.LineContactId` 是**聯絡用 LINE ID**（`@john_kol`），與 OAuth 登入用的 LINE `userId`（`Uxxxxxxxx`）是兩個不同的東西，不可混用。

LINE Notify 已於 2025 年停止服務，推播須改用 **LINE Messaging API**。

---

#### 五、時程表重估結論

- 原估算 107 小時為**樂觀下限**
- 每個「畫面功能」實際含所有層（Application / Infrastructure / Controller）平均約 3～6 小時
- 以下項目**在原時程表中完全沒有編列**：
    - LINE Login / LIFF 整合
    - LINE Messaging API 推播架構
    - `ReferralOrders` 導購訂單串接
    - 帳務規則（鎖款 / 解凍 / 手續費）完整實作
    - 月結撥款流程（含發票上傳）
    - 通知偏好設定（KOL-005）

**待確認後更新時程的開會議題**

- [ ] 導購訂單 API 規格（訂房系統窗口）
- [ ] LINE 整合方式（LIFF + Messaging API 確認）
- [ ] 帳務規則文件（R05～R08）補齊
- [ ] KOL-005 通知設定頁的 LINE 推播事件類型清單

---

### [10:12] LINE Login 整合方向討論中（待開會確認）

**討論背景**

KOL 端設定為在 LINE 上註冊並開啟，確認需要串接 LINE Login（OAuth 2.0）。

**已確認方向：方案 A — LINE Login（OAuth 登入）**

KOL 點「用 LINE 登入」按鈕，走 OAuth 2.0 flow，系統拿到 LINE 的 `userId` 建立帳號。

**需要的 Schema 變更（已分析，尚未執行）**

1. **新增 `UserExternalLogins` 資料表**（必要）
    - 欄位：`Id / UserId / Provider('LINE') / ProviderUserId(Uxxxxxxx) / CreatedAt`
    - 唯一索引：`(Provider, ProviderUserId)`
    - 用途：OAuth callback 後，用 LINE userId 找對應的 Users 記錄

2. **`Users.Email` 改為 nullable**（必要）
    - LINE Login 不保證給 email（用戶可拒絕授權），現為 `NOT NULL` 會導致無法建帳號

3. **`KolProfiles.LineContactId` 定位**（已定案）
    - 此欄位與 `UserExternalLogins.ProviderUserId` 是不同的東西
    - `UserExternalLogins.ProviderUserId`：系統 ID（`U47b7fc46a7b...`），OAuth 登入用
    - `KolProfiles.LineContactId`：使用者自己的聯絡 LINE ID（`@john_kol`），給業者聯絡用
    - 已採用：保留聯絡欄位並改名為 `LineContactId`
    - 選 B：刪除（平台內溝通即可）

4. **`Users.Email` 設計策略**（待確認）
    - 選 A：Email nullable，KOL 可在個人資料頁另外填
    - 選 B：強制 KOL 首次登入後必須填寫 email（視為必填）

**需要的架構新增（已分析，尚未執行）**

| 層              | 需要新增                                                               |
| --------------- | ---------------------------------------------------------------------- |
| Application     | `IExternalAuthService` 介面（`ExchangeCodeAsync`、`GetUserInfoAsync`） |
| Infrastructure  | LINE Login OAuth 實作（呼叫 LINE API 換 token、取 profile）            |
| Kol/Controllers | `AccountController` 補 `LineLogin` + `LineCallback` 兩個 Action        |

**⏳ 等待開會確認後再執行**

- [x] `KolProfiles.LineContactId` 已定案為聯絡用 LINE ID
- [ ] `Users.Email` nullable + 個人資料補填 or 強制首次登入填寫？

---

## 2026-06-26

### [18:48] ADM-005 KOL 管理後端完成

**變更內容**

**Schema**

- `KolProfiles` 補欄位：Phone / LineContactId / AcceptsCash / AcceptsBarter / AcceptsCommission / RejectionNote / SuspensionNote
- 新增 `KolCategories`（KOL 類型多選，26 個類別）
- 新增 `KolSocialAccounts`（社群帳號，11 個平台，含驗證狀態/資料來源）
- 新增 `KolBankAccounts`（收款資料，個人/公司）
- 索引區補 4 個新索引

**Domain**

- 新增 Entity：KolProfile / KolCategory / KolSocialAccount / KolBankAccount
- `Errors.Kol` 補 5 個錯誤碼

**Application DTOs（10 個）**

- KolListItemDto / KolReviewListItemDto / KolDetailBaseDto / KolDetailDto
- KolSocialAccountDto / KolBankAccountDto / KolTaskSummaryDto
- KolEarningsSummaryDto / KolActivityLogDto / KolStatsDto

**Application Repository 介面（4 個）**

- IKolRepository（GetListAsync / GetReviewListAsync / GetDetailBaseAsync / UpdateAsync）
- IKolSocialAccountRepository（GetByKolId）
- IKolBankAccountRepository（GetByKolId，帳號遮蔽）
- IKolStatsRepository（統計 / 收益 / 近期任務 / 活動 / 類型）

**Application Queries / Commands（7 組）**

- GetKolListQuery + Handler（ADM-005）
- GetKolReviewListQuery + Handler（ADM-015）
- GetKolDetailQuery + Handler（ADM-006/016，平行查詢組裝）
- ApproveKolCommand + Handler（Pending/Rejected → Approved，通過後清除 RejectionNote）
- RejectKolCommand + Handler（Pending → Rejected，必填 RejectionNote）
- SuspendKolCommand + Handler（→ Suspended，必填 SuspensionNote）
- UnsuspendKolCommand + Handler（Suspended → Approved，清除 SuspensionNote）

**Admin**

- ViewModels：KolListQueryViewModel / KolReviewListQueryViewModel / KolRejectViewModel / KolSuspendViewModel
- KolManagementController（Index / ReviewList / Detail / Approve / Reject / Suspend / Unsuspend）
- DependencyInjection.cs 補 7 個 KOL Handler 註冊

**整個 Solution 建置：0 錯誤**

**決策原因**

- 可合作類型確認為三類（現金酬勞/體驗項目/導購分潤），對應 RewardType Enum
- KOL 停權原因選 B（存 KolProfiles.SuspensionNote），方便詳情頁直接讀取
- 重送審核時直接把 VerificationStatus 改回 Pending（不另外記錄送審次數）
- 業者/KOL 停用是否同時鎖 Users.Status，待 PM 確認後補實作（目前只改 VerificationStatus）

---

## 2026-06-26

### [16:17] ADM-004 業者管理後端 — 步驟 3～7 全部完成

**變更內容**

**步驟 3 — GetMerchantDetailQuery / Handler**

- `Application/Merchants/DTOs/`：新增 8 個 DTO（MerchantContactDto / MerchantStatsDto / MerchantCaseSummaryDto / MerchantWalletSummaryDto / MerchantWalletTransactionDto / MerchantMemberItemDto / MerchantActivityLogDto / MerchantDetailDto / MerchantDetailBaseDto）
- `IMerchantRepository`：補 `GetDetailBaseAsync`
- `IMerchantWalletRepository`：補 `GetRecentTransactionsAsync`
- `IMerchantMemberRepository`：補 `GetMemberListAsync`
- `IMerchantContactRepository`：新建（GetByMerchantId / Insert / Update / Delete / BelongsToMerchant）
- `IMerchantStatsRepository`：新建（GetStats / GetRecentCases / GetRecentActivityLogs）
- `GetMerchantDetailQuery` + `GetMerchantDetailHandler`：平行查詢 7 個子集合後組裝

**步驟 4 — Suspend / Unsuspend**

- `schema.sql`：`VerifiedByAdminId` 改名為 `UpdatedByAdminId`
- `Domain/Entities/Merchant.cs`：屬性同步改名
- `Errors.Merchant`：補 `AlreadySuspended` / `NotSuspended`
- `SuspendMerchantCommand + Handler`：Approved/Rejected → Suspended
- `UnsuspendMerchantCommand + Handler`：Suspended → Approved

**步驟 5 — UpdateMerchant**

- `schema.sql`：`Merchants` 補 7 欄（EnglishName / IndustryType / Fax / CompanyEmail / Website / Address / EstablishedDate）
- `Domain/Entities/Merchant.cs`：屬性同步補上
- `MerchantDetailBaseDto / MerchantDetailDto`：欄位同步補上
- `UpdateMerchantCommand + Handler`：編輯業者基本資料

**步驟 6 — Contact CRUD**

- `AddMerchantContactCommand + Handler`：確認業者存在 → Insert → 回傳新 Id
- `UpdateMerchantContactCommand + Handler`：BelongsToMerchant 防越權 → Update
- `RemoveMerchantContactCommand + Handler`：BelongsToMerchant 防越權 → Delete

**步驟 7 — MerchantManagementController**

- `Admin/ViewModels/Merchant/`：新增 3 個 ViewModel（MerchantListQueryViewModel / MerchantUpdateViewModel / MerchantContactViewModel）
- `Admin/Controllers/MerchantManagementController.cs`：新建（Index / Detail / Suspend / Unsuspend / Update / AddContact / UpdateContact / RemoveContact 共 8 個 Action）
- `Application/DependencyInjection.cs`：註冊 8 個新 Handler
- 整個 Solution 建置：**0 錯誤**

**決策原因**

- 業者不走審核流程（email 驗證即可），`VerificationStatus` 在業者端只用 Approved / Suspended
- `VerifiedByAdminId` 改名為 `UpdatedByAdminId`，語意更準確（記錄最後操作的 Admin）
- 業者停用不需要填原因（可能是倒閉等情況），原因由 ActivityLog 另行記錄
- MerchantContacts 獨立一張表，支援多位聯絡窗口
- 聯絡窗口操作前先做 `BelongsToMerchantAsync` 防越權

---

### 待繼續

| #     | 功能                                  | Controller                     |
| ----- | ------------------------------------- | ------------------------------ |
| ✅ 1  | 登入 / 登出                           | `AccountController`            |
| ✅ 4  | 業者管理（列表 / 詳情 / 停用 / 編輯） | `MerchantManagementController` |
| ⏳ 2  | 後台帳號管理                          | `AdminAccountController`       |
| ⏳ 3  | RBAC 角色與權限設定                   | `RolePermissionController`     |
| ⏳ 5  | KOL 管理（列表 / 審核）               | `KolManagementController`      |
| ⏳ 6  | 案件監控                              | `CaseMonitorController`        |
| ⏳ 7  | 爭議處理                              | `DisputeController`            |
| ⏳ 8  | 帳務管理                              | `FinanceController`            |
| ⏳ 9  | 系統參數設定                          | `SystemSettingController`      |
| ⏳ 10 | 營運總覽 Dashboard                    | `DashboardController`          |

**尚未實作（Infrastructure 層）**

- `MerchantRepository`（GetListAsync / GetDetailBaseAsync 的 SQL）
- `MerchantWalletRepository`（GetRecentTransactionsAsync SQL）
- `MerchantMemberRepository`（GetMemberListAsync SQL）
- `MerchantContactRepository`（全部 CRUD SQL）
- `MerchantStatsRepository`（統計 SQL）

---

## 2026-06-25（晚間）

### [19:06] ADM-004 業者管理後端 — 步驟 1～2 完成

**變更內容**

- `schema.sql`：在「3. 業者組織與成員」區塊末端新增 `MerchantContacts` 資料表
    - 欄位：`Id / MerchantId / Name / Phone / Email / Title / Note / CreatedAt`
    - FK：`FK_MerchantContacts_Merchant → Merchants(Id)`
    - 索引：`IX_MerchantContacts_MerchantId`
- `Application/Abstractions/Repositories/IMerchantRepository.cs`：補 `GetListAsync(keyword, verificationStatus, page, session, ct)` 方法
- `Application/Merchants/DTOs/MerchantListItemDto.cs`：新建（列表列項 DTO）
    - 欄位：MerchantId、CompanyName、TaxId、ContactName、Phone、OwnerEmail、VerificationStatus、CreatedAt、AvailableAmount、CaseCount
- `Application/Merchants/Queries/GetMerchantListQuery.cs`：新建（sealed record，含 Keyword / VerificationStatus / Page / PageSize）
- `Application/Merchants/Queries/GetMerchantListHandler.cs`：新建（Handler，呼叫 `IMerchantRepository.GetListAsync`，回傳 `PagedResult<MerchantListItemDto>`）
- 建置驗證：**Application 專案 0 錯誤 0 警告**

**決策原因**

- `MerchantContacts` 獨立一張表支援多位聯絡窗口（Merchants 原本只有單筆 ContactName/Phone）
- `GetListAsync` 回傳 Tuple `(Items, TotalCount)` 讓 Repository 直接做 COUNT，Handler 組裝 `PagedResult`

---

### 待繼續（步驟 3～8）

| #   | 步驟                                                                   | 狀態    |
| --- | ---------------------------------------------------------------------- | ------- |
| 1   | `schema.sql` 補 `MerchantContacts`                                     | ✅ 完成 |
| 2   | `GetMerchantListQuery / Handler`                                       | ✅ 完成 |
| 3   | `GetMerchantDetailQuery / Handler`                                     | ⏳ 待做 |
| 4   | `ApproveMerchantCommand / Handler` + `RejectMerchantCommand / Handler` | ⏳ 待做 |
| 5   | `SuspendMerchantCommand / Handler`                                     | ⏳ 待做 |
| 6   | `UpdateMerchantCommand / Handler`                                      | ⏳ 待做 |
| 7   | `AddMerchantContactCommand / Handler` + `RemoveContact`                | ⏳ 待做 |
| 8   | `MerchantManagementController`（所有 Actions）                         | ⏳ 待做 |

**步驟 3 開始點**：需建立 `MerchantDetailDto`（聚合：基本資料 + 聯絡窗口 + 統計 + 案件 + 錢包 + 成員 + 活動），並補 `IMerchantRepository.GetDetailAsync` + 新增 `IMerchantContactRepository`。

---

### [18:47] Admin 登入功能完成（全部 9 步）

**變更內容**

- `Infrastructure/DependencyInjection.cs`：新增 `IUserRepository → UserRepository`、`IRoleRepository → RoleRepository`（Scoped）
- `Application/DependencyInjection.cs`：新增 `LoginHandler`（Scoped）
- `Admin/Controllers/AccountController.cs`：新建
    - `GET /Account/Login`：`[AllowAnonymous]`，已登入自動轉 Dashboard
    - `POST /Account/Login`：`[AllowAnonymous]` + `[EnableRateLimiting(RateLimitPolicies.Login)]`，建立 Claims（NameIdentifier / Name / account_type / permission×N），SignInAsync，RememberMe=8h/否則 60min Sliding
    - `POST /Account/Logout`：SignOutAsync → 跳回 Login

**決策原因**

- 類別層級 `[AllowAnonymous]` 會蓋掉方法層級 `[Authorize]`（ASP0026 警告），改為各 Login action 個別掛 `[AllowAnonymous]`
- `Error.Message` 不存在（正確屬性為 `Description`），已修正
- Logout 不需要 `[Authorize]`，未登入執行 SignOutAsync 是 no-op，安全無虞

---

### [18:31] Admin 登入 — Application 層完成

**變更內容**

- `Admin/ViewModels/Account/LoginViewModel.cs`：密碼最短改為 8 字元
- `Domain/Exceptions/Errors.cs`：`Errors.User` 補 `InvalidCredentials`、`AccountSuspended`、`NotAdminAccount`
- `Application/Abstractions/Repositories/IRoleRepository.cs`：補 `GetPermissionCodesByUserIdAsync`
- `Application/Account/LoginCommand.cs`：新建，含 `LoginCommand` + `LoginResult`
- `Application/Account/LoginHandler.cs`：新建，完整登入邏輯

**LoginHandler 驗證流程**

1. Email 查 User → 找不到回 `InvalidCredentials`
2. AccountType 必須為 Admin → 否則 `NotAdminAccount`
3. Status = Suspended → `AccountSuspended`
4. Status = Deleted → `InvalidCredentials`（不洩漏帳號是否存在）
5. PasswordHash 為 null（第三方帳號）或密碼不符 → `InvalidCredentials`
6. 載入 Permission Codes（UserRoles → RolePermissions → Permissions）
7. 回傳 `LoginResult`（UserId、Name、Email、PermissionCodes）

**決策原因**

- 讀取用 `IUnitOfWork.BeginAsync()` 取得 Session，不 Commit，auto-rollback 無害
- `InvalidCredentials` 同時用於「帳號不存在」和「密碼錯誤」，避免枚舉攻擊
- `Deleted` 帳號回傳 `InvalidCredentials` 而非 `AccountSuspended`，對外不暴露帳號狀態

---

**變更內容**

- `CONTRIBUTING.md` 新增 §15 業務規則速查表（9 張表）：
    - 獎勵類型組合規則、業者案件操作按鈕規則
    - 完整案件/報名/任務狀態切換表
    - KOL 與業者任務操作權限表、完整驗收狀態切換表
- `Domain/Enums/SubmissionStatus.cs`：補 `Overdue = 5`、`Disputed = 6`
- `Domain/Enums/TaskStatus.cs`：移除 `Disputed = 9`（爭議只在 Submission 層管理）
- `Domain/Entities/Submission.cs`：補 `CanDispute()` Domain 規則方法
- `schema.sql`：
    - Tasks 表：移除 9=Disputed 註解，CHECK 從 `(1..9)` 改為 `(1..8)`
    - Submissions 表：補 5=Overdue、6=Disputed 註解，CHECK 從 `(1..4)` 改為 `(1..6)`
- 建置：**0 錯誤**

**決策原因**

- Rules §R04 驗收狀態表明確有 `Overdue`（逾期）與 `Disputed`（爭議中）兩個 Submission 狀態
- Rules §R03 任務狀態表沒有 `Disputed`，爭議狀態改由 Submission 層統一追蹤，Task 維持 Incomplete
- 來源文件：`rules_utf8.htm`（原 BIG5 → UTF8 轉換版）

---

**變更內容**

- `copilot-instructions.md` 新增以下章節：
    - **分批確認規則**：每完成一個檔案/段落就停下等確認，不可一次生成整個功能
    - **C# 命名規則**：Async 後綴、底線前綴、sealed record/class
    - **Controller Action 模板**：GET 查詢頁 + POST 寫入操作完整範本
    - **ErrorType → HTTP 對應表**：NotFound/Forbidden/Conflict/Validation/Problem
    - **Use Case Handler 結構**：Command/Query + sealed Handler + Constructor Injection
    - **Repository Dapper 慣例**：QueryFirstOrDefaultAsync / QueryAsync / ExecuteScalarAsync / ExecuteAsync

**決策原因**

- 這些是 AI 生成程式碼時每次都要遵守的慣例，放 `copilot-instructions.md` 最有效
- 分批確認規則是使用者明確需求：「不要一次完成太多程式碼，一部分就讓我確認」

---

---

## 2026-06-25

### [17:15] 開發階段規範與 Admin 功能開發順序確認

**變更內容**

- 更新 `copilot-instructions.md` 專案區，新增「當前開發階段：後端優先」章節
- 明定規則：View 暫不建立、ViewModel 完整定義、Handler 完整實作、Repository 完整 SQL
- 記錄每個功能的實作檔案清單（ViewModel → Command → Handler → Repository → Controller）
- 記錄 Admin 功能開發順序（共 10 項，從登入開始）

**決策原因**

- 前端畫面尚未完成，先把後端做好，等畫面有了可以直接接上
- 「View 暫不做」屬於 AI 每次對話都要遵守的規則，放 `copilot-instructions.md` 最有效
- 「建議執行順序」屬於進度規劃，記錄於 MEMORY.md
- Admin 功能開發完成後可順便用後台新建業者與 KOL 測試資料，因此優先做 Admin

**Admin 功能開發順序**

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

### [15:48] 專案初始化與文件建立

**變更內容**

- 確認專案性質：KOL 任務媒合平台，三角色（KOL / Merchant / Admin），各自獨立 MVC 網站
- 建立 `.github/CONTRIBUTING.md`：完整架構規範（14 節），含分層職責、Use Case、狀態機、RBAC、Log、外部設定
- 建立 `schema.sql`：29 張資料表的完整 MSSQL 建表 SQL，含 PK / FK / CHECK / INDEX
- 原 `ARCHITECTURE-DRAFT.md` 已由使用者刪除，以 `CONTRIBUTING.md` 為唯一架構文件

**決策原因**

- 命名統一用 `Merchant`（非 `vendor`），與 C# Domain 一致
- 資料表命名用 `PascalCase`，與 CONTRIBUTING 第 9 節規範一致
- `ApplicationStatus` 新增 `PendingReconfirmation`（3）與 `Invalid`（6），對應案件修改後 KOL 重新確認流程
- `Submissions` 新增 `IsAutoApproved` 與 `ReviewDeadlineAt`，對應 14 天自動驗收機制
- RBAC 相關表（Roles / Permissions / RolePermissions / UserRoles / MerchantMembers / MerchantInvitations）為 Excel 舊版缺漏，已補入 schema.sql
- 不使用 `appsettings.json`，設定存外部 JSON 不進版控（參考舊專案 CorporateSite.json 結構）
- Log 採 Serilog，DEMO 環境輸出 Console + File，正式環境只輸出 File

**尚待確認（見 CONTRIBUTING 第 14 節）**

- 預估凍結金額完整公式
- `Incomplete` / `Cancelled` Task 是否算入結束條件
- 爭議額度釋放規則
- 外部設定檔路徑規範與 CI/CD 注入方式

---

### [16:00] Solution 結構規劃（已確認，尚未執行）

**已確認的決策**

- Solution 名稱：`TaskSystem.sln`
- 所有專案**不加前綴**，短名稱：`Kol` / `Merchant` / `Admin` / `Application` / `Domain` / `Infrastructure` / `Common`
- 設定檔共用一個：`Account/TaskSystem.json`（放在方案根目錄的 `Account/` 資料夾，不進版控）
- 三個 MVC 的 `Program.cs` 都讀同一路徑 `../Account/TaskSystem.json`

**規劃的資料夾結構**

```text
c:\旅圖\任務系統\
├─ .github\
├─ Account\               ← 外部設定檔（不進版控）
│   └─ TaskSystem.json
├─ Kol\                   ← ASP.NET Core MVC
├─ Merchant\              ← ASP.NET Core MVC
├─ Admin\                 ← ASP.NET Core MVC
├─ Application\           ← Class Library
├─ Domain\                ← Class Library
├─ Infrastructure\        ← Class Library
├─ Common\                ← Class Library
├─ TaskSystem.sln
├─ .gitignore
└─ schema.sql
```

**專案參考關係（依 CONTRIBUTING 第 4 節）**

```text
Kol        → Application、Infrastructure
Merchant   → Application、Infrastructure
Admin      → Application、Infrastructure
Application → Domain、Common
Infrastructure → Application、Domain、Common
Domain     → Common（僅在確實需要時）
Common     → 無
```

**狀態：已完成（2026-06-25 16:01）**

所有步驟已執行完畢，見下方 [16:01] 記錄。

---

### [16:30] 三個 MVC Program.cs 設定完成

**變更內容**

- 新增 `Application/DependencyInjection.cs`（stub）
- 新增 `Infrastructure/DependencyInjection.cs`（stub）
- 改寫 `Kol/Program.cs`、`Merchant/Program.cs`、`Admin/Program.cs`
- 修正 `Account/TaskSystem.json`：補 `IsDEMO`、連線字串 key 改為 `DefaultConnection`、Serilog MinimumLevel 結構對齊 CONTRIBUTING 13.1

**每個 Program.cs 共同結構**

1. 讀外部設定 `../Account/TaskSystem.json`（`Path.GetFullPath` 處理相對路徑）
2. Serilog：File 恆開，`IsDEMO=true` 時加 Console
3. `AddApplication()` + `AddInfrastructure(config)` DI stub
4. Cookie Auth：各自獨立 Cookie Name，`HttpOnly = true`，`SecurePolicy = Always`
5. Middleware：`UseAuthentication` → `UseAuthorization`

**三站差異**

| 專案     | Cookie Name            | Session         |
| -------- | ---------------------- | --------------- |
| Kol      | `.TaskSystem.Kol`      | 480 分鐘        |
| Merchant | `.TaskSystem.Merchant` | 480 分鐘        |
| Admin    | `.TaskSystem.Admin`    | 60 分鐘（較短） |

**補裝套件**

- `Application`：`Microsoft.Extensions.DependencyInjection.Abstractions 10.0.9`
- `Infrastructure`：同上 + `Microsoft.Extensions.Configuration.Abstractions 10.0.9`

**決策原因**

- classlib 不含 ASP.NET Core，需要明確加 Abstractions 套件才能使用 `IServiceCollection` / `IConfiguration`
- Cookie 名稱各站獨立，避免 Admin Session 被 KOL/Merchant Cookie 干擾（CONTRIBUTING 3.3）
- `AccountType` Claim 驗證留待 Auth Filter / Middleware 建立後實作（TODO）

---

### [16:14] Domain 層 Enum + Entity 骨架建立

**變更內容**

- 刪除 4 個 classlib 的預設 `Class1.cs`
- 新增 `Domain/Enums/` 16 個 Enum 檔案
- 新增 `Domain/Entities/` 10 個 Entity 骨架

**Enum 清單（含對應 DB 欄位值）**

| 檔案                   | 說明                                                     |
| ---------------------- | -------------------------------------------------------- |
| `AccountType`          | Admin=1, Merchant=2, Kol=3                               |
| `UserStatus`           | Active=1, Suspended=2, Deleted=3                         |
| `VerificationStatus`   | Pending=1, Approved=2, Rejected=3, Suspended=4           |
| `RoleScope`            | System=1, Merchant=2                                     |
| `MerchantMemberStatus` | Active=1, Suspended=2, Removed=3                         |
| `CaseStatus`           | Draft=1…Cancelled=7                                      |
| `RecruitmentStatus`    | NotOpen=1, Open=2, Closed=3, Paused=4                    |
| `TaskStatus`           | PendingMatch=1…Disputed=9                                |
| `ApplicationStatus`    | Applied=1…Invalid=6                                      |
| `SubmissionStatus`     | Submitted=1, RevisionRequested=2, Approved=3, Rejected=4 |
| `RewardType`           | Cash=1, Commission=2, Barter=3                           |
| `KolEarningStatus`     | Pending=1…Cancelled=6                                    |
| `KolEarningSourceType` | CashReward=1, Commission=2, Adjustment=3                 |
| `PayoutRequestStatus`  | Pending=1…Cancelled=5                                    |
| `DisputeStatus`        | Open=1…Cancelled=6                                       |
| `SocialPlatform`       | Instagram=1…Blog=6                                       |

**Entity 清單**

`Case`, `CaseTask`, `Application`, `Submission`, `Merchant`, `MerchantMember`, `Role`, `Permission`, `MerchantWallet`, `KolEarning`

- 骨架含對應 DB 欄位（public get/set）與少量 Domain 規則方法 stub
- `TaskStatus` 與 `System.Threading.Tasks.TaskStatus` 衝突，以 `using TaskStatus = Domain.Enums.TaskStatus;` alias 解決

**決策原因**

- Entity 命名 `CaseTask`（非 `Task`），以避免與 `System.Threading.Tasks.Task` 名稱衝突
- 骨架 Entity 使用 `public { get; set; }` 便於 Dapper 映射 DTO 後轉換，後續視需求加私有 setter 或工廠方法
- `RewardType` 定義為案件「報酬機制分類」，與 `KolEarningSourceType` 的「收益來源」分開

---

### [16:01] Solution 結構建立完成

**變更內容**

- 建立 `TaskSystem.sln`
- 建立 7 個專案：
    - MVC：`Kol`、`Merchant`、`Admin`（ASP.NET Core MVC, net9.0）
    - Class Library：`Application`、`Domain`、`Infrastructure`、`Common`
- 設定 Project Reference（依 CONTRIBUTING 第 4 節）：
    - Kol / Merchant / Admin → Application、Infrastructure
    - Application → Domain、Common
    - Infrastructure → Application、Domain、Common
- 安裝 NuGet 套件：
    - Infrastructure：`Dapper 2.1.79`、`Microsoft.Data.SqlClient 7.0.1`
    - Kol / Merchant / Admin：`Serilog.AspNetCore 10.0.0`、`Serilog.Sinks.File 7.0.0`、`Serilog.Sinks.Console 6.1.1`
- 建立各專案子資料夾（ViewModels、Filters、Extensions、Abstractions/…、Cases/…、Domain 各層等），以 `.gitkeep` 維持空目錄
- 建立 `.gitignore`（排除 `Account/`、`**/logs/`、`bin/`、`obj/`、`.vs/` 等）
- 建立 `Account/TaskSystem.json` 外部設定檔範本（含 DB、Cookie、Serilog、Platform 設定）
- `dotnet build TaskSystem.sln` 驗證：**7 個專案全部建置成功，零錯誤**

**決策原因**

- `Account/` 放在方案根目錄，三個 MVC 的 `Program.cs` 日後讀 `../Account/TaskSystem.json`（此路徑在 CONTRIBUTING 中已定義）
- `.gitignore` 的 `appsettings.*.json` 排除策略保留 `appsettings.json`，環境特定設定由外部 JSON 取代
- 子資料夾結構完全對應 CONTRIBUTING 3.1～3.7 各節規範

---

### [16:21] Common 層 Result / Error 結構建立

**變更內容**

- `Common/Errors/ErrorType.cs`：None / NotFound / Validation / Conflict / Forbidden / Problem / Unexpected
- `Common/Errors/Error.cs`：immutable sealed class，工廠方法（`NotFound` / `Validation` / `Conflict` / `Forbidden` / `Problem`），實作 `IEquatable<Error>`
- `Common/Results/Result.cs`：無回傳值結果，`Success()` / `Failure(Error)`
- `Common/Results/ResultT.cs`：`Result<TValue>`，隱式轉換支援 `return value;` / `return error;`
- `Common/Pagination/PagedResult.cs`：含 `TotalPages` / `HasPreviousPage` / `HasNextPage`
- `Common/Pagination/PageQuery.cs`：`Page` / `PageSize` / `Offset`，自動 clamp（最大 100）
- `Common/Guards/Guard.cs`：`AgainstNull` / `AgainstNullOrWhiteSpace` / `AgainstNegative` / `AgainstZeroOrNegative` / `AgainstPastDate` / `AgainstInvalidRange`
- `Domain/Exceptions/Errors.cs`：集中定義所有預定義業務錯誤碼（Case / Application / Submission / Task / Merchant / Member / Role / Wallet / Payout / Dispute / User）
- `Domain.csproj` 新增 `Common` reference（`Errors.cs` 確實需要）

**決策原因**

- `Error` 設計為 sealed + private constructor，強制透過工廠方法建立，確保 Type 分類一致
- 隱式轉換讓 Application Use Case 可直接 `return CaseErrors.NotFound;` 而不需包裝
- 錯誤碼集中在 `Domain/Exceptions/Errors.cs`，避免字串散落各處，Controller 可用 `ErrorType` 對應 HTTP 狀態碼
- `Domain → Common` 的 reference 符合 CONTRIBUTING 第 4 節「僅在確實需要時」的規範

---

### [16:28] 高優先資安對策實作

**變更內容**

- 安裝 `BCrypt.Net-Next 4.2.0`（Infrastructure）
- 新增 `Application/Abstractions/Security/IPasswordHasher.cs`（介面）
- 新增 `Infrastructure/Authentication/BcryptPasswordHasher.cs`（WorkFactor=12，約 250ms）
- `Infrastructure/DependencyInjection.cs`：註冊 `IPasswordHasher → BcryptPasswordHasher`（Singleton）
- 三個 `Program.cs` 加入 `Cookie.SameSite`（Kol/Merchant = Lax，Admin = Strict）
- 三個 `Program.cs`：`AddControllersWithViews` 加入 `AutoValidateAntiforgeryTokenAttribute`（全域 CSRF）
- 新增 `Application/Abstractions/Security/ICurrentUser.cs`（目前登入者介面）
- 三個 MVC 各自新增 `Extensions/HttpContextCurrentUser.cs`（從 Claims 讀取）
- 三個 `Program.cs`：註冊 `AddHttpContextAccessor()` + `ICurrentUser → HttpContextCurrentUser`（Scoped）

**決策原因**

- `IPasswordHasher` 介面放 Application，Application 不感知 BCrypt 細節，未來可換演算法
- WorkFactor=12 是目前業界建議值，定期應隨硬體提升而調高
- Admin Cookie 用 `SameSite.Strict`（更嚴格），Kol/Merchant 用 `Lax`（允許從外部連結進入後保持登入）
- `AutoValidateAntiforgeryTokenAttribute` 全域套用，若某端點不需要（如 API webhook）可加 `[IgnoreAntiforgeryToken]` 例外
- `ICurrentUser` 注入 Use Case，讓 Application 層可驗證 Ownership 而不需要直接讀 HttpContext

**Use Case 使用方式（未來實作）**

```csharp
// Application Use Case 建構式注入
public sealed class PublishCaseHandler(
    ICurrentUser currentUser,
    ICaseRepository caseRepository)
{
    public async Task<Result> HandleAsync(PublishCaseCommand cmd)
    {
        if (!currentUser.HasPermission("Merchant.Case.Publish"))
            return Result.Failure(Errors.User.Forbidden);

        var caseEntity = await caseRepository.GetByIdAsync(cmd.CaseId);
        if (caseEntity is null)
            return Result.Failure(Errors.Case.NotFound);

        // Ownership 驗證：案件必須屬於操作者所在的 Merchant
        if (caseEntity.MerchantId != cmd.MerchantId)
            return Result.Failure(Errors.User.Forbidden);
        // ...
    }
}
```

**登入時 Claims 寫入方式（未來實作）**

```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new("account_type", user.AccountType.ToString()),
};
// 把 Permission Code 逐一加入
foreach (var perm in userPermissions)
    claims.Add(new Claim("permission", perm.Code));
```

---

### [16:34] 中優先資安對策：Rate Limiting

**變更內容**

- `Common/Primitives/RateLimitPolicies.cs`：Policy 名稱常數（`rl_login` / `rl_forgot_password` / `rl_global`）
- `Account/TaskSystem.json`：加入 `RateLimit` 設定區塊（可調參數）
- 三個 `Program.cs`：加入 `AddRateLimiter()` + `UseRateLimiter()`，使用內建 Sliding Window 演算法

**三站 Rate Limit 差異**

| Policy         | Kol / Merchant | Admin                    |
| -------------- | -------------- | ------------------------ |
| Login          | 60 秒 / 5 次   | 60 秒 / **3 次**（更嚴） |
| ForgotPassword | 300 秒 / 3 次  | 300 秒 / 3 次            |
| Global         | 10 秒 / 100 次 | 10 秒 / 100 次           |

**Controller 使用方式（未來）**

```csharp
[EnableRateLimiting(RateLimitPolicies.Login)]
[HttpPost("login")]
public async Task<IActionResult> Login(LoginViewModel vm) { ... }

[EnableRateLimiting(RateLimitPolicies.ForgotPassword)]
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm) { ... }
```

**決策原因**

- 使用 .NET 8+ 內建 `RateLimiter`，不需要額外套件
- Sliding Window 演算法比 Fixed Window 更平滑，避免視窗邊界突發請求
- Policy 名稱集中在 `Common/Primitives/RateLimitPolicies.cs`，避免三站各自硬編碼字串
- Admin 登入限制更嚴格（3 次 vs 5 次），對應後台安全性更高的要求
- 設定值放 `TaskSystem.json` 可不改程式碼就調整閾值

---

### [16:38] Infrastructure Persistence 層建立

**變更內容**

- `Application/Abstractions/Persistence/ISqlConnectionFactory.cs`：回傳 `IDbConnection`（System.Data），Application 不依賴 SqlClient
- `Application/Abstractions/Persistence/IDbSession.cs`：封裝 `IDbConnection` + `IDbTransaction`
- `Application/Abstractions/Persistence/IUnitOfWork.cs`：`BeginAsync()` / `CommitAsync()` / `RollbackAsync()`，實作 `IAsyncDisposable`
- `Infrastructure/Persistence/Dapper/SqlConnectionFactory.cs`：從 `DefaultConnection` 建立 `SqlConnection`，回傳 `IDbConnection`
- `Infrastructure/Persistence/Transactions/DbSession.cs`：`IDbSession` 具體實作（internal sealed）
- `Infrastructure/Persistence/Transactions/UnitOfWork.cs`：`IUnitOfWork` 具體實作，`DisposeAsync` 時未 Commit 自動 Rollback
- `Infrastructure/DependencyInjection.cs`：補上 `ISqlConnectionFactory`（Singleton）、`IUnitOfWork`（Scoped）

**決策原因**

- `ISqlConnectionFactory` 回傳 `IDbConnection` 而非 `SqlConnection`，讓 Application 層不需引用 `Microsoft.Data.SqlClient`，符合依賴反轉
- `IUnitOfWork` 使用 `await using` 模式，離開 using 區塊時若未 Commit 則自動 Rollback，防止遺忘回滾
- `DbSession` 標記 `internal sealed`，外部只透過 `IDbSession` 介面存取，不暴露具體實作
- `ISqlConnectionFactory` 用 Singleton（連線字串固定），`IUnitOfWork` 用 Scoped（每個 Request 一個 Transaction 邊界）

**Use Case 使用模式（未來）**

```csharp
public sealed class PublishCaseHandler(IUnitOfWork unitOfWork, ICaseRepository caseRepo, ...)
{
    public async Task<Result> HandleAsync(PublishCaseCommand cmd)
    {
        await using var uow = await unitOfWork.BeginAsync();

        var caseEntity = await caseRepo.GetByIdAsync(cmd.CaseId, uow.Session);
        // ... 業務邏輯 ...
        await caseRepo.UpdateAsync(caseEntity, uow.Session);
        await walletRepo.UpdateAsync(wallet, uow.Session);

        await uow.CommitAsync();
        return Result.Success();
    }
}
```

---

### [16:46] Application Repository 介面建立

**變更內容**

- `Domain/Entities/User.cs`：補建（`IUserRepository` 需要）
- `Application/Abstractions/Repositories/` 新增 9 個介面：

| 介面                        | 對應 Entity / 資料表               |
| --------------------------- | ---------------------------------- |
| `ICaseRepository`           | `Case` / Cases                     |
| `ITaskRepository`           | `CaseTask` / Tasks                 |
| `IApplicationRepository`    | `Application` / CaseApplications   |
| `ISubmissionRepository`     | `Submission` / Submissions         |
| `IMerchantRepository`       | `Merchant` / Merchants             |
| `IMerchantMemberRepository` | `MerchantMember` / MerchantMembers |
| `IMerchantWalletRepository` | `MerchantWallet` / MerchantWallets |
| `IKolEarningRepository`     | `KolEarning` / KolEarnings         |
| `IRoleRepository`           | `Role` / Roles                     |
| `IUserRepository`           | `User` / Users                     |

**每個介面的方法設計原則**

- 所有方法接受 `IDbSession session` 參數，確保參與 Transaction
- 所有方法接受 `CancellationToken ct = default`
- 寫入類：`InsertAsync`（回傳新 ID）、`UpdateAsync`
- 讀取類：`GetByIdAsync`、業務用途查詢（`GetByMerchantAndUserAsync` 等）
- 批次操作：`InsertManyAsync`、`UpdateManyAsync`（案件取消、修改用）
- `ICaseRepository` 加入 `GetByIdAndMerchantAsync`（Ownership 防護）

**命名衝突說明**

- `IApplicationRepository` 的方法參數型別需全名 `Domain.Entities.Application`，避免與 namespace `Application` 衝突
- `IUserRepository` 同理，型別用 `Domain.Entities.User`

**決策原因**

- Repository 只負責資料存取，不含業務判斷（符合 CONTRIBUTING 8.3）
- `IMerchantWalletRepository` 的查詢備註 `WITH (UPDLOCK)` 提醒實作時需行鎖，防止並發超扣
- `ISubmissionRepository.GetOverdueAsync` 為自動驗收排程專用查詢入口
