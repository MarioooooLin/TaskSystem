# TaskSystem Contributing Guide

本文件是後端實作總規範。頁面層級的 Figma 規格已拆到 `.github/specs/`，本文件保留架構原則、共通流程、狀態規則、驗收方式與待確認事項。

## 1. 架構原則

- 專案採 ASP.NET Core MVC + Application / Domain / Infrastructure 分層。
- Controller 只處理 HTTP、Model Binding、Auth 與結果轉換。
- Application 層負責 Use Case、交易邊界、授權檢查與呼叫 Repository。
- Domain 層負責狀態規則、業務判斷與 Enum 語意。
- Infrastructure 層負責 Dapper、SQL、外部服務與檔案存取。
- SQL / Repository 不應自行決定業務流程，例如案件是否可發布、任務是否完成。
- 所有寫入流程需有明確 Command；查詢流程使用 Query / DTO，不直接回傳 Entity 給 View。

### 1.1 外部設定檔

- Admin、Merchant、Kol 三個 MVC 站台共用方案根目錄下的 `Account/TaskSystem.json`，不得將連線字串、Cookie、Serilog、平台參數等共用或機密設定重複寫入各站台的 `appsettings.json`。
- 三個站台啟動時皆須透過 `AddTaskSystemExternalConfiguration()` 載入 `../Account/TaskSystem.json`；該檔案為必要設定（`optional: false`），開發與部署環境都必須在啟動前提供。
- ASP.NET Core 仍會載入各站台的 `appsettings.json` 與環境設定檔；`TaskSystem.json` 於其後加入，因此同名設定以 `TaskSystem.json` 為準。
- `Account/` 屬外部機密資料夾並由 `.gitignore` 排除，不得提交 `TaskSystem.json` 或其中的密鑰、連線字串與憑證。

## 2. 專案職責

### 2.1 Admin

- 管理業者、KOL、案件監控、異議、帳務、系統參數、後台帳號與角色權限。
- Admin 不受業者前台按鈕限制，但所有人工處理都必須檢查 Permission 並寫入操作紀錄。

### 2.2 Merchant

- 業者端只能操作目前登入成員所屬的 Merchant。
- 所有 Merchant Query / Command 必須從登入者推導 `MerchantId`，不得信任 query string 指定其他業者。
- 業者端角色與權限透過 `MerchantMembers`、`Roles`、`RolePermissions` 判斷。

### 2.3 Kol

- KOL 端只能存取自己的個人資料、任務、提交成果、收益與通知。
- 收款資料屬敏感資料，後台與 KOL 端皆需預設遮罩。

## 3. 寫入與交易規則

- 一個 Use Case 若會更新多張表，必須在同一 transaction 內完成。
- 金流、錢包、折扣金、收益與結算不可只覆寫餘額；必須保留交易流水或結算快照。
- 操作紀錄至少需包含操作者、目標 Id、動作、前後狀態、時間與備註。
- 高風險操作包含停權/解除停權、代理登入、帳務調整、角色權限異動、審核結果、異議處理；匯出功能若未來恢復，也需列為高風險操作並寫入稽核紀錄。

## 4. 身份與授權

- 授權不能只依畫面是否顯示按鈕。
- Controller 可先檢查 Permission；Application Use Case 仍須再次驗證：
  - 操作者是否擁有所需 Permission。
  - MerchantMember 是否屬於目標 Merchant 且狀態為 Active。
  - 操作者是否擁有該 Case。
  - KOL 是否綁定該 Task。
  - Admin 是否具有人工處理權限。
- 後台帳號可同時擁有多個 System roles。
- 停用後台帳號需立即讓既有後台 session 失效。
- 不允許停用、刪除或移除最後一個具備最高管理權限的後台帳號或角色。

## 5. Figma 頁面規格

PM/Figma 畫面是後端規格來源之一。新增功能時，優先以 Figma 頁面代號定位需求，再參考時程表與《台灣旅圖 KOL 任務系統運作規則》補齊流程與狀態。

頁面層級規格已拆出：

| 使用端 | 規格檔 | 範圍 |
| ------ | ------ | ---- |
| Admin 後台 | [admin-pages.md](specs/admin-pages.md) | `ADM-*` 後台頁面、KOL 審核、案件監控、帳務、系統設定、帳號與角色權限 |
| Merchant 業者端 | [merchant-pages.md](specs/merchant-pages.md) | `MER-*` 業者端 Shell、首頁與後續業者端頁面 |
| KOL 端 | [kol-pages.md](specs/kol-pages.md) | `KOL-*` LIFF 入口、案件、任務、收益與個人資料 |

每個 Figma 頁面應整理：

| 欄位 | 說明 |
| ---- | ---- |
| 頁面目的 | 使用者在此頁完成的工作 |
| 主要區塊 | 畫面上的資料區、表格、卡片、按鈕 |
| 查詢資料 | Query、DTO、資料來源 |
| 提交動作 | Command、交易、狀態異動 |
| 權限 | Permission code 與資料範圍 |
| 資料表 | 既有表與需新增表 |
| 狀態規則 | Enum、推導規則、禁止操作 |
| 稽核紀錄 | ActivityLog、交易流水、使用紀錄 |

`ADM-002 業者管理頁` 目前 Figma 與 KOL 畫面混用，暫不採用。

## 6. 開發與驗收

- 目前不要求新增自動化測試。
- 功能完成時至少需能說明手動驗收方式、資料一致性檢查方式，並在環境允許時執行 `dotnet build TaskSystem.sln`。
- 若 `dotnet build` 因 NuGet、SSL、公司網路或套件還原失敗，需記錄為環境問題，不因此更動業務程式碼。
- Figma 看不清的欄位、必填規則、按鈕語意，不得自行擴大解釋；先標記 `待確認`。
- 若 Figma 與運作規則衝突，先以 Figma 畫面整理實作需求，再在 `待確認` 記錄衝突點。

## 7. 已定案共通規則

- 業者資料編輯 `ADM-004` 必填：公司/法人名稱、統一編號、公司電話、公司信箱。
- 帳號註冊的登入憑證欄位，業者端與管理者端第一版皆為 Email + 密碼；業者公司資料、管理者姓名/部門/角色等屬於各端延伸資料，不屬於登入憑證最低欄位。
- 登入帳號密碼不設強制複雜度限制；系統可計算並顯示密碼強度/複雜度提示，但不得因強度不足阻擋送出。唯一阻擋條件為密碼未填或確認密碼不一致，儲存時依使用者設定的密碼建立 `Users.PasswordHash`。
- 多語系第一版只做 UI 介面文案多語系，不做使用者輸入內容多語系。固定系統文案使用 `.resx` 或等價資源檔管理；案件名稱、案件簡介、KOL 自介、業者名稱、備註、審核意見與申訴理由等使用者輸入內容，以原文保存與顯示，不新增內容翻譯資料表。
- 所有匯出功能第一版暫緩實作。Figma 若有匯出按鈕，可在 UI 保留停用/隱藏占位，但後端不建立匯出 Command、不產生 CSV/Excel/PDF，不需確認匯出欄位；未來恢復時需重新定義欄位、權限、格式與稽核紀錄。
- LINE Login 若未提供 Email，KOL 首次登入需強制補填 Email 後才能建立或完成 `Users` 帳號；`Users.Email` 維持必填且全系統唯一。
- KOL 停權同步更新 `KolProfiles.VerificationStatus = Suspended` 與 `Users.Status = Suspended`。
- KOL 解除停權先預設回到 `KolProfiles.VerificationStatus = Approved` 與 `Users.Status = Active`。
- KOL 收款帳號在後台永遠遮罩，不顯示完整帳號。
- `ADM-005` KOL 管理統計摘要不跟隨搜尋與篩選，只套用資料權限。
- `ADM-007` 四張風險卡都作為 `RiskType` 快速篩選。
- `ADM-008` 頁籤僅作為同頁區塊定位跳轉，不代表後端分頁或分段載入。
- `ADM-008` 批次操作功能第一版移除；後台案件詳情與進度頁僅提供單筆查看、單筆爭議處理、附件下載等操作。
- `ADM-009` 業者端操作叫「退回補件」，後台任務狀態叫「待補件」；`待補件` 不新增 `DisputeStatus`。
- `ADM-011` 目前沒有獨立帳務狀態，狀態欄以案件狀態為主；未完成帳務、異議或導購同步異常以警示或阻擋結案規則處理。
- `ADM-011` 預設統計期間為當月。帳務監控需區分「資金流入/流出」與「營運收入/支出」：業者儲值屬資金流入，不直接等同平台營收；案件結算、平台服務費、固定開案費與人工調整才作為營運收入口徑。
- `ADM-012` 參數儲存後立即對未來資料生效；既有案件、訂單、任務與結算依各自快照，不回溯改寫。
- 預估凍結金額公式：`EstimatedFrozenAmount = (RewardAmountPerKol * WantedKolCount * KolServiceFeeRate) + CaseOpeningFeeAmount`。其中單人/組 KOL 費用與預計組數來自案件，KOL 服務費率與固定開案費來自 Admin 系統參數，並需保存於 `CaseBudgetSnapshots` 快照。
- 導購佣金最低比例公式：`MinimumCommissionRate = AffiliatePlatformCommissionRate + AffiliateKolMinCommissionRate`。業者在開案時輸入的 `CommissionRate` 是總佣金比例，必須大於或等於最低比例；平台固定取得 `AffiliatePlatformCommissionRate`，扣除平台抽成後的剩餘比例全數歸 KOL。若業者輸入剛好等於最低比例，KOL 取得 `AffiliateKolMinCommissionRate`；若輸入超過最低比例，超出的部分也歸 KOL。
- `ADM-013` 後台帳號延伸資料使用 `AdminProfiles`；邀請使用通用 `UserInvitations`。
- `ADM-014` 角色說明補 `Roles.Description`，系統保留補 `Roles.IsSystemReserved`，高風險權限補 `Permissions.RiskLevel`。
- 審核流程採共通語意但不共用 enum：待審核、退回修改、通過、不通過、逾期、爭議中可作為前端文案與規格對照；KOL 資料審核使用 `VerificationStatus`，任務成果驗收使用 `SubmissionStatus` 與 `TaskStatus`。
- KOL 資料審核中的 `VerificationStatus.Rejected` 定義為「退回修改/已退回待補」，不是永久拒絕；任務成果驗收中的 `SubmissionStatus.Rejected` 才代表「驗收不通過」。
- `ADM-015` KOL 審核流程不新增新的 `VerificationStatus`；使用 `KolReviewEvents` 記錄送審、重送、通過、退回事件，列表上的「待審核 / 重送審核 / 已退回待補」由 `KolProfiles.VerificationStatus` 加最新審核事件推導。
- `KolProfiles.VerificationStatus = Pending` 且最新送審事件不是退回後重送時，顯示「待審核」；若最新送審/重送事件發生在退回事件之後，顯示「重送審核」；`Rejected` 顯示「已退回待補」；`Approved` 不列入待審清單。
- 平台金額、錢包、鎖定額度、KOL 收益與撥款狀態皆作為後台營運與財務線下收付款依據；系統不直接執行實際收款或匯款，但仍需保留示意帳務流水、狀態、操作者、時間與備註供對帳稽核。
- 爭議成立或結案時，系統只更新示意帳務狀態、鎖定/釋放額度、KOL 應付收益或人工調整紀錄；管理者與財務依系統結果在線下處理實際收款、退款或匯款。
- 業者停權採整體停權：停用業者主檔後，該業者底下所有角色帳號皆不可登入或操作業者端。業者復權第一版採整體復權：恢復業者主檔，並恢復因該業者停權而停用的成員帳號；若未來需保留成員個別停用狀態，需新增停權前狀態快照或明確操作紀錄。
- 折扣金採獨立 `MerchantCreditWallets` / `MerchantCreditTransactions`，不混入現金錢包；只有業者有折扣金，且第一版只能折抵案件開案費，不可折抵 KOL 酬勞、導購分潤、儲值或其他款項。所有加值、扣回、折抵開案費、退回、到期與人工調整都必須走折扣金交易流水。
- `MER-003` 招募中只要修改案件內容，即觸發已錄取 KOL 重新確認；確認期限天數待 PM 確認。
- `MER-004` 平台服務費率以 Admin 系統參數為準，Figma 數字僅為示意。
- `MER-009` 驗收通過時必須完成 KOL 評分；驗收不通過不自動建立異議，需使用者提出申訴才建立 `Disputes`。
- `MER-015` 業者查看 KOL 詳細資料時，Email 完整顯示。
- `MER-010` 交易明細暫不做匯出。
- `MER-HOME` 餘額不足待辦只在業者嘗試發布案件且可用餘額不足時建立；首頁不主動掃描草稿或待發布案件產生餘額不足待辦。
- `MER-HOME` 不存在待審核案件狀態；若 Figma 或靜態切版出現待審核案件卡片，視為多出的示意狀態，不新增 `CaseStatus.PendingReview`。
- `MER-010` 充值第一版只支援銀行轉帳/ATM，由後台或人工對帳確認入帳，不串接第三方金流。
- `MER-010` 充值表單的統一編號與 Email 用於付款通知與帳務對帳，不作為發票開立規則。
- `MER-014` 有效 KOL 數定義為導購金額大於 0 的 KOL；訂單總金額需扣除退款、取消與異常交易。
- `MER-014` 導購資料由外部系統定期傳入本系統資料庫，業者端使用者不直接觸發外部同步。
- `MER-016` 查看外部訂單採只讀連結。
- `MER-008`「使用者與角色設定」與「使用者角度與權限設定」分別代表成員管理與角色權限管理。
- `MER-018` 業者端修改統一編號、公司名稱等敏感公司資料，不需後台重新審核。
- `MER-018` 聯絡窗口至少需保留一筆主要聯絡人。
- `MER-012` 強制通知事件為安全、付款失敗、系統維護，不可關閉。
- `MER-012` 通知偏好採公司層級設定。
- `KOL-005` 與 `MER-012` 通知偏好採通用 `NotificationPreferences`，以 `OwnerType` 區分個人使用者與業者公司層級設定；通知事件仍需建立 `Notifications` 或寫入操作紀錄，偏好表只決定是否發送指定通道。
- `MER-011` Owner 只能有一位；第一版不限制席位數。
- `MER-011` 重新寄送邀請時，取消舊邀請並建立新邀請。
- `MER-011` Owner/Admin/Member 採固定三角色權限矩陣，但支援業者自訂新增角色。
- `MER-011` 系統保留角色不可改名，但可改描述；角色權限異動後需立即套用並讓既有 session 權限失效重載。
- `MER-011` 預設角色權限矩陣第一版：Owner 具備全部業者端權限；Admin 可管理案件、KOL、錢包充值、企業資料與通知偏好，但不可轉移 Owner 或管理角色權限；Member 僅具備首頁、案件、KOL、導購成效與企業資料的查看權限。此矩陣作為 seed data 預設值，後續可依營運需求調整或由業者自訂角色覆蓋。
- `MER-018` Figma 頁面代號重複暫以 `MER-018-A` / `MER-018-B` 區分。
- `MER-019` 註冊完成 Email 驗證後直接啟用業者帳號，並需要重寄驗證信頁面。
- `KOL-002` 服務地區與擅長語言採獨立資料表，例如 `KolServiceAreas`、`KolLanguages`。
- `KOL-002` 收款帳戶送審可不填，提領/收益結算前才強制補齊。
- `KOL-002` 平台使用條款同意先在 `KolProfiles` 記錄同意時間，暫不做條款版本表。
- `KolProfiles.LineContactId` 只保存 KOL 自行填寫的聯絡用 LINE ID，不可作為 LINE Login、LIFF OAuth 或 Messaging API 推播識別；真正的 LINE `userId` 需保存於 `UserExternalLogins.ProviderUserId` 或等價外部登入綁定表。
- `KOL-003` 社群平台以 `schema.sql` 11 種平台為準，補齊 `Domain.Enums.SocialPlatform`。
- `KOL-003` 社群資料預計採購外部 API 並以排程同步，每週同步一次。
- `KOL-004` 收款與稅務資料拆表：新增 `KolTaxProfiles` 與 `KolTaxDocuments`，`KolBankAccounts` 只管銀行帳戶。
- `KOL-004` KOL 本人可點眼睛短暫查看完整銀行帳號，需寫入敏感資料查看紀錄；不新增獨立敏感紀錄表，使用擴充後的 `ActivityLogs`；後台仍永遠遮罩。
- `KOL-006` 推薦任務排序：匹配度優先，其次截止日期，再其次更新時間。
- `KOL-007` / `KOL-008` KOL 條件不符合仍允許報名，但需顯示不符合原因。
- `KOL-008` 報名成功後導向我的任務/報名紀錄。
- `KOL-009` KOL 執行中也可直接放棄任務，但需留下紀錄，並可能影響評分。
- `KOL-009` KOL 對業者評分在案件結案後必填。
- `KOL-011` 成果提交必填規則依交付規範決定；有連結型成果必填連結，有截圖要求才必填附件。
- `KOL-014` 新增月結批次/結算單資料表保存每月快照。
- `KOL-014` 新增 `KolSettlementItems` 固定結算單與 `KolEarnings` 的關聯，避免歷史月結金額漂移。
- `KOL-014` 新增 `PayoutRequestDocuments` 保存勞報單、發票影本、簽署狀態與上傳檔案。
- `KOL-014` 提領門檻寫入系統參數，實作為 `>= 1000` 可提領。
- `KOL-014` 文件流程：系統產生勞報單供個人戶簽署回傳，公司戶上傳發票影本；KOL 上傳後需 Admin 審核通過才可撥款。
- `KOL-014` 勞報單與發票相關文件模板由財務提供；系統需保存模板版本與帶入欄位快照，實際版型與保存年限待財務模板與法遵規則到位後補齊。

## 8. 業務規則速查表

### 8.1 獎勵類型組合規則

| 規則 | 說明 |
| ---- | ---- |
| 任務可有多個獎勵類型 | 同一任務可同時包含現金酬勞、體驗項目、導購分潤 |
| 任務至少需有一個獎勵類型 | 沒有獎勵類型的任務不得成立 |
| 系統依類型啟用規則 | 掛載現金酬勞則啟用錢包規則；掛載導購分潤則啟用導購規則 |
| 體驗項目不啟用金流 | 體驗項目僅記錄提供內容，不產生平台現金鎖款 |

### 8.2 案件狀態

| 狀態 | 說明 |
| ---- | ---- |
| 草稿 | 尚未發布 |
| 招募中 | KOL 可報名，業者可接受 KOL |
| 招募截止 | 報名時間結束，不再接受新報名 |
| 執行中 | 案件已進入執行階段 |
| 已完成 | 案件底下所有必要任務皆結束 |
| 已結案 | 所有帳務與流程完成 |
| 已取消 | 案件終止 |

### 8.3 業者案件操作按鈕規則

| 案件狀態 | 編輯案件 | 發布案件 | 案件詳情 | 查看導購成效 |
| -------- | -------- | -------- | -------- | ------------ |
| 草稿 | 是 | 是 | 是 | 否 |
| 招募中 | 是 | 是 | 是 | 否 |
| 招募截止 | 是 | 是 | 是 | 否 |
| 執行中 | 否 | 否 | 是 | 視是否啟用導購分潤 |
| 已完成 | 否 | 否 | 是 | 視是否啟用導購分潤 |
| 已結案 | 否 | 否 | 是 | 視是否啟用導購分潤 |
| 已取消 | 否 | 否 | 是 | 視是否啟用導購分潤 |

管理者不受此表限制，可依權限進行人工處理。

### 8.4 案件狀態切換

| 目前狀態 | 觸發動作 | 下一狀態 |
| -------- | -------- | -------- |
| 草稿 | 業者發布案件，系統依預定招募數量建立任務 | 招募中 |
| 招募中 | 招募期限到期 | 招募截止 |
| 招募中 | 業者主動進入執行中 | 執行中 |
| 招募截止 | 錄取人數 >= Ceiling(預定招募數量 / 2)（自動） | 執行中 |
| 招募截止 | 未達自動執行規則，業者主動進入執行中 | 執行中 |
| 執行中 | 所有必要任務完成 | 已完成 |
| 已完成 | 帳務完成 | 已結案 |
| 草稿 / 招募中 / 招募截止 | 取消案件 | 已取消 |

### 8.5 自動執行規則

- 自動執行只在招募時間結束時由系統判斷。
- 只有 `ApplicationStatus.Accepted` 計入錄取人數。
- 自動執行條件：`AcceptedApplicationCount >= Ceiling(PlannedTaskCount / 2)`。
- Merchant 手動開始案件不受自動執行規則限制。

### 8.6 任務與驗收

- 任務建立時機：案件發布時，依預定招募數量建立任務。
- 任務初始狀態：建立後初始為待媒合。
- 案件完成：代表所有必要任務完成。
- 任務驗收屬於任務層級流程。
- 案件取消會影響所有未完成任務。
- 業者端「退回補件」對應後台任務狀態「待補件」，不新增 `DisputeStatus`。

## 9. 尚待確認

### 9.1 通用規則

- `Incomplete` 或 `Cancelled` Task 是否算入「所有必要任務已結束」。

### 9.2 KOL / 審核

- `ADM-006` 多社群平台驗證狀態不一致時，列表與詳情摘要顯示規則。
- `ADM-015` 資料完整度計算公式。
- `KOL-005` LINE 通知需依 LINE Messaging API userId 推播；需確認 LINE 綁定與推播失敗 fallback。

### 9.3 案件 / 導購 / 帳務

- 導購同步異常、訂單統計與預估分潤需依 `ReferralOrders` 或等價資料表補齊，資料來源需確認。
- `ADM-011` 平台總收入、平台總支出、平台毛利的最終營運口徑需由財務確認；目前文件先採第一版建議公式。

### 9.4 業者端

- `MER-016` 導購訂單交易狀態需確認是否沿用可結算、處理中、異常交易、已取消。
- `MER-020` 缺少輸入新密碼頁與重設完成頁。
- 業者端缺頁候選：邀請接受/設定密碼頁、個人帳號設定頁、通知中心列表頁、客服/說明中心頁。
