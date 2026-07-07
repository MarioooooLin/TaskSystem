# Merchant Figma Page Specs

> This file is referenced by `.github/CONTRIBUTING.md`. It contains Merchant-side Figma page specs translated into backend implementation notes.

## MER-000 業者端頁面容器 Shell

**頁面目的**

- 作為業者端共用頁面容器，提供側邊導覽、頁首業者資訊、錢包餘額、通知與帳號入口。
- 所有業者端頁面都必須依目前登入者與所屬業者組織決定資料範圍，不得透過 query string 任意切換其他業者資料。

**主要區塊**

- 側邊導覽：首頁、案件管理、錢包與充值、導購成效、導購明細、設定。
- 頁首資訊：業者公司名稱、目前登入成員角色、業者或帳號啟用狀態、錢包可用餘額、通知入口、帳號選單入口。
- 內容插槽：各業者端頁面放入主要內容。

**查詢資料**

- `GetMerchantShellQuery`
  - 回傳目前登入者的業者基本資料、成員角色、可用導覽項目、錢包摘要、未讀通知數與版本資訊。
  - 主要來源：`Users`、`Merchants`、`MerchantMembers`、`Roles`、`RolePermissions`、`MerchantWallets`、`Notifications`。

**資料與權限規則**

- 業者端目前登入者必須屬於有效 `MerchantMembers`，且 `Users.AccountType = Merchant`。
- 若 `Users.Status != Active` 或 `MerchantMembers.Status != Active`，不得進入業者端。
- 側邊導覽依角色權限顯示；沒有權限的項目不可只前端隱藏，後端也需拒絕。
- 錢包餘額只顯示目前業者自己的 `MerchantWallets` 彙總。
- 通知只顯示目前使用者或目前業者範圍內通知。

**稽核**

- Shell 查詢不寫操作紀錄。
- 點擊通知、切換帳號狀態、登出等互動若後續有狀態變更，才寫操作紀錄。

## MER-HOME 業者首頁

**頁面目的**

- 提供業者登入後的營運總覽，包含錢包摘要、案件狀態統計、待辦事項與最近案件。
- 此頁主要為查詢與導向入口，不直接修改案件或金流資料。

**主要區塊**

- 頁首操作：前往充值、新增案件。
- 錢包摘要：可用餘額、已預定金額、錢包總金額。
- 案件狀態統計：草稿、待審核、招募中、執行中、待驗收、已結案。
- 待辦事項：待辦類型、案件名稱、狀態、建立時間、操作。
- 最近案件：案件名稱、狀態、數據、總預算、操作。
- 預留區塊：未來指標或圖表。

**查詢資料**

- `GetMerchantHomeQuery`
  - 回傳錢包摘要、案件統計、待辦事項、最近案件與頁面操作權限。
  - 必須以目前登入者可操作的 `MerchantId` 作為資料範圍。
- 主要資料來源：
  - `Merchants`：業者資料與狀態。
  - `MerchantMembers`、`Roles`、`RolePermissions`：目前成員角色與可用操作。
  - `MerchantWallets`、`MerchantWalletTransactions`：可用餘額、已預定金額與錢包總金額。
  - `Cases`、`CaseApplications`、`Tasks`、`Submissions`、`Disputes`：案件統計、待辦事項與最近案件。
  - `CaseBudgetSnapshots`：案件預算與已預定金額快照。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 前往充值 | 導向業者錢包與充值頁，不在首頁建立交易。 |
| 新增案件 | 導向案件建立流程。 |
| 查看 | 導向對應案件詳情或待辦項目詳情。 |
| 前往驗收 | 導向案件任務驗收區或待驗收任務列表。 |

**狀態與資料規則**

- 首頁案件統計只包含目前業者自己的案件。
- 待驗收以 `Submissions.Status = Submitted` 或等價待審核成果狀態判斷。
- 錢包可用餘額不得手算覆寫，需由錢包主檔或交易流水彙總產生。
- 已預定金額對應現有 `MerchantWallets.FrozenAmount`，代表已被案件預算、待執行任務、爭議保留或結算流程占用但尚未釋放/撥款的金額。
- 錢包總金額對應 Domain 既有 `MerchantWallet.TotalAmount` 規則：`AvailableAmount + FrozenAmount`。
- 最近案件排序依 `Cases.CreatedAt` 倒序，顯示筆數待確認。
- 待辦事項只顯示需要業者行動的項目，例如成果待驗收、餘額不足、案件待補資料。
- 餘額不足待辦只在業者嘗試發布案件且可用餘額不足時建立；首頁不主動掃描草稿或待發布案件產生餘額不足待辦。
- 業者端案件沒有待審核狀態；若 Figma 或靜態切版出現待審核案件卡片，視為多出的示意狀態，不新增 `CaseStatus.PendingReview`。

**權限**

- 查看首頁需 `Merchant.Home.View` 或等價登入權限。
- 前往充值需 `Merchant.Wallet.TopUp`。
- 新增案件需 `Merchant.Case.Create`。
- 查看案件與驗收需依案件權限檢查，例如 `Merchant.Case.View`、`Merchant.Task.Review`。

**稽核**

- 首頁查詢不寫操作紀錄。
- 點擊導向不寫操作紀錄；實際充值、建立案件、驗收等 Command 由目標頁寫入操作紀錄。

## MER-002 案件管理頁

**頁面目的**

- 讓業者查詢、篩選、建立與追蹤自己名下的合作案件。
- 本頁所有資料都必須以目前登入的 `MerchantId` 為 scope，不可查到其他業者案件。

**主要區塊**

- 狀態摘要：草稿、待審核、招募中、執行中、待驗收、已結案。
- 搜尋篩選：關鍵字、案件狀態、合作條件、發布平台、日期區間。
- 案件列表：案件名稱、狀態、合作條件、發布平台、報名/招募、任務/待驗、總預算、日期截止、操作。
- 操作按鈕：新增案件、編輯案件、發佈案件、案件詳情、查看導購成效。

**查詢資料**

- `GetMerchantCaseListQuery`
  - Input：`MerchantId`、keyword、status、conditionTypes、platforms、dateRange、page、pageSize。
  - Output：狀態摘要、篩選後案件列表、分頁資訊。
  - 資料來源：`Cases`、`CaseBudgetSnapshots`、`CaseApplications`、`Tasks`、`Submissions`、`ReferralCampaigns`。

**提交動作**

- `CreateMerchantCaseDraftCommand`：點擊新增案件時建立草稿或導向空白編輯頁。
- `PublishMerchantCaseCommand`：僅草稿或可發佈狀態可用，實際扣鎖金額應在 MER-004 確認頁完成。
- `OpenMerchantCaseDetailQuery`：進入 MER-005。
- `OpenMerchantReferralReportQuery`：進入導購成效頁，僅導購分潤案件顯示。

**狀態與權限**

- 草稿、招募中、招募截止可顯示編輯案件；執行中之後僅允許有限欄位編輯，詳見 MER-003。
- 查看導購成效只適用於啟用導購分潤的案件。
- 業者後台使用者需具備 `Merchant.Case.View`；新增、編輯、發佈需具備 `Merchant.Case.Manage`。

## MER-005 案件詳情頁

**頁面目的**

- 顯示單一案件完整資訊、KOL 報名與任務狀態，並提供案件階段操作。

**主要區塊**

- 案件標頭：案件名稱、案件編號、業者名稱、案件狀態。
- 頂部操作：返回案件管理、編輯案件、查看導購成效、發出案件、進行執行、取消案件。
- 摘要卡：報名人數、已接受 KOL、待驗收、爭議處理中、已完成、案件總預算。
- 左側資訊：案件資訊、合作條件、交付要求、附件。
- 右側清單：KOL 清單與任務狀態，支援狀態、平台與更新時間排序。
- 空狀態：尚無報名資料。

**查詢資料**

- `GetMerchantCaseDetailQuery`
  - Input：`MerchantId`、`CaseId`。
  - Output：案件主檔、預算快照、合作條件、交付需求、附件、KOL 應募/任務清單、摘要統計。
  - 資料來源：`Cases`、`CaseBudgetSnapshots`、`CaseConditions`、`CasePlatforms`、`CaseDeliverables`、`CaseAttachments`、`CaseApplications`、`Tasks`、`Submissions`、`Disputes`。

**KOL 清單欄位**

- KOL 名稱、主要平台、粉絲數、任務或報名狀態、最近更新、操作。
- 操作依狀態顯示：查看 KOL 資料、接受、拒絕、查看驗收、確認交付。

**提交動作**

- `StartMerchantCaseExecutionCommand`：業者手動將可執行案件進入執行中。
- `CancelMerchantCaseCommand`：取消案件，需依案件狀態釋放鎖定款項並記錄操作。
- `AcceptKolApplicationCommand` / `RejectKolApplicationCommand`：接受或拒絕報名 KOL。
- `OpenSubmissionReviewQuery`：進入 MER-009 成果驗收頁。

**狀態規則**

- 自動進入執行中的後端規則以 CONTRIBUTING 已定案規則為準：招募時間結束時，錄取人數 `>= Ceiling(預計招募人數 / 2)`。
- Figma 圖上的提醒文案若出現「報名人數」或「預定招募人數達到 50%」等字樣，實作時應改寫為已定案的錄取人數規則。
- 爭議處理中案件不可結案；異議處理完成後才可進入結案流程。

## MER-003 新增/編輯案件頁

**頁面目的**

- 讓業者建立草稿、編輯案件內容，並在送出發布前預估需鎖定的錢包額度。

**主要區塊**

- 基本資料：案件名稱、案件簡介、官方網站/參考連結、執行地區、詳細地址。
- 時間與招募：欲求數量、報名截止日期、稿件交付截止。
- 合作條件：現金酬勞、體驗項目、導購分潤設定。
- KOL 條件：粉絲門檻、KOL 類型、擅長語言。
- 平台與交付：發布平台、交付需求清單。
- 附件檔案：參考素材、制式合約範本。
- 預算預估：單人現金酬勞、組數、小計、平台服務費、總計扣除額度、目前可用餘額。

**欄位規則**

- 必填：案件名稱、案件簡介、執行地區、詳細地址、欲求數量、報名截止日期、稿件交付截止、現金酬勞、KOL 類型、擅長語言、發布平台、交付需求清單。
- 體驗項目可多筆新增，儲存為案件合作條件的一部分。
- 導購分潤開啟後才需要佣金比例與追蹤天數；關閉時不可建立導購活動。
- 導購分潤開啟時，業者輸入的佣金比例為總佣金比例 `CommissionRate`，必須大於或等於 Admin 參數 `AffiliatePlatformCommissionRate + AffiliateKolMinCommissionRate`。
- 導購分潤拆分：平台固定取得 `AffiliatePlatformCommissionRate`，KOL 取得 `CommissionRate - AffiliatePlatformCommissionRate`；若業者輸入超過最低比例，超出的比例全數歸 KOL。
- 粉絲門檻為篩選條件，後端需保留原始門檻值，供 KOL 搜尋與案件詳情呈現。

**查詢資料**

- `GetMerchantCaseEditQuery`
  - 新增時回傳預設值、系統參數、業者錢包可用餘額。
  - 編輯時回傳案件既有資料、附件、預算快照與可編輯欄位清單。
- `PreviewMerchantCaseBudgetQuery`
  - 依現金酬勞、欲求數量、平台服務費率、導購設定計算預估鎖定額度。
  - 預估凍結金額公式：`EstimatedFrozenAmount = (RewardAmountPerKol * WantedKolCount * KolServiceFeeRate) + CaseOpeningFeeAmount`。
  - `RewardAmountPerKol` 為單人/組 KOL 費用，`WantedKolCount` 為預計組數，`KolServiceFeeRate` 與 `CaseOpeningFeeAmount` 來自 Admin 系統參數。
  - 回傳導購佣金最低比例、平台抽成比例與 KOL 預估可得比例，供前端顯示與即時驗證。

**提交動作**

- `SaveMerchantCaseDraftCommand`：儲存草稿，不鎖定錢包金額。
- `ContinueMerchantCasePublishCommand`：驗證必填與預算預估後進入 MER-004。
- `UpdateMerchantCaseCommand`：更新既有案件，需依狀態限制可改欄位。

**編輯限制**

- 草稿、待審核、招募中可完整編輯。
- 案件進入執行中後，僅允許調整不影響 KOL 權益與任務條件的欄位，例如案件名稱、簡介、素材附件；金額、招募數、日期、合作條件、交付需求預設不可改。
- 招募中只要業者修改案件內容，即觸發已錄取 KOL 重新確認；系統將相關 `CaseApplications.Status` 更新為 `PendingReconfirmation`。
- `PendingReconfirmation` 需有確認期限，但天數待 PM 確認。

## MER-004 發佈案件確認頁

**頁面目的**

- 發布前讓業者確認案件內容與預算鎖定金額，避免發布後再變動核心條件。

**主要區塊**

- 重要確認事項：發布後系統會依預算金額鎖定錢包額度；需返回草稿才可重新編輯。
- 案件摘要：案件名稱、地區、招募人數、執行期間、案件簡介。
- 合作條件摘要：現金酬勞、體驗項目、導購分潤。
- 平台與交付摘要：發布平台、交付需求。
- 附件摘要：附件名稱、大小、類型。
- 預算鎖定摘要：案件總預算、平台服務費、預計鎖定總額。
- 最後確認：內容正確、理解錢包將被鎖定。

**查詢資料**

- `GetMerchantCasePublishConfirmationQuery`
  - Input：`MerchantId`、`CaseId`。
  - Output：案件快照、預算試算、錢包可用餘額、確認項目。

**提交動作**

- `ConfirmPublishMerchantCaseCommand`
  - 驗證案件仍可發布。
  - 驗證必填確認 checkbox。
  - 驗證 `MerchantWallets.AvailableBalance >= LockedAmount`。
  - 建立或更新預算快照。
  - 以交易流水鎖定錢包金額，不可只覆寫餘額。
  - 案件狀態由草稿轉為招募中或待審核，依後端審核流程設定。
  - 寫入操作紀錄。

**資料一致性**

- 預算鎖定必須具備 idempotency key，避免重複點擊造成重複鎖款。
- KOL 服務費率與固定開案費需來自 Admin 系統參數，不可寫死在前端；Figma 上百分比與金額僅視為示意數字。
- 鎖定金額、KOL 服務費率、固定開案費與計算明細需保存為發布當下快照，後續費率調整不得影響已發布案件。

## MER-009 成果驗收頁

**頁面目的**

- 業者檢視 KOL 交付成果，決定驗收通過、退回修改或驗收不通過，並可對 KOL 進行評分。

**主要區塊**

- 案件摘要：案件名稱、截止日期、合作條件、KOL 名稱、提交時間。
- 創作者留言：KOL 對成果的補充說明。
- 交付物列表：平台、標題、連結、互動數據、查看連結。
- 原始圖檔與附件：預覽、下載。
- 審核意見：必填輸入框。
- 動作按鈕：返回上頁、退回修改、驗收不通過、驗收通過。
- KOL 評分 modal：綜合評分、KOL 配合度、任務執行度、內容品質、溝通順暢度、合作推薦度、評分留言。

**查詢資料**

- `GetMerchantSubmissionReviewQuery`
  - Input：`MerchantId`、`CaseId`、`TaskId` 或 `SubmissionId`。
  - Output：案件摘要、任務資料、KOL 資料、提交成果、附件、既有審核紀錄。
  - 資料來源：`Cases`、`Tasks`、`Submissions`、`SubmissionItems`、`SubmissionFiles`、`KolProfiles`、`KolSocialAccounts`。

**提交動作**

- `ApproveSubmissionCommand`
  - 將提交狀態更新為驗收通過。
  - 驗收通過時必須完成 KOL 評分，並建立 `Reviews` / `ReviewScores`。
  - 將任務狀態推進為已完成或待結算。
  - 建立 KOL 應付金額與後續撥款資料。
  - 寫入使用紀錄與操作紀錄。
- `RequestSubmissionRevisionCommand`
  - 將提交狀態更新為退回修改。
  - 任務在管理端應呈現為待補件。
  - 保存退回原因，不建立應付金額。
- `RejectSubmissionCommand`
  - 將提交狀態更新為驗收不通過。
  - 保存拒絕原因；不直接建立異議，需由使用者提出申訴後才建立 `Disputes`。
- `CreateKolReviewCommand`
  - 儲存業者對 KOL 的評分與文字留言。

**狀態與隱私**

- 成果驗收狀態對應：
  - 待驗收：`SubmissionStatus.Submitted`，任務狀態為 `TaskStatus.UnderReview`。
  - 修改中：`SubmissionStatus.RevisionRequested`，任務狀態為 `TaskStatus.RevisionRequested`。
  - 驗收通過：`SubmissionStatus.Approved`，任務狀態為 `TaskStatus.Completed`。
  - 驗收不通過：`SubmissionStatus.Rejected`，任務狀態為 `TaskStatus.Incomplete`。
  - 驗收逾期：`SubmissionStatus.Overdue`，任務狀態依規則維持 `TaskStatus.Incomplete` 或進入爭議處理。
  - 爭議中：`SubmissionStatus.Disputed`，並以未結案 `Disputes` 追蹤。
- 「退回修改」不等同成果驗收不通過；不得將退回修改寫成 `SubmissionStatus.Rejected`。
- 驗收通過、退回修改、驗收不通過都必須寫入審核意見。
- 評分資料只作為 KOL 評價與後續推薦依據，若需公開顯示需另外確認顯示範圍。
- 業者只能驗收自己案件底下的任務成果。

## MER-015 KOL 詳細資料頁

**頁面目的**

- 讓業者在案件報名或 KOL 名單中檢視 KOL 資料，並針對案件報名做接受或拒絕決策。

**主要區塊**

- KOL 標頭：名稱、評分、KOL 狀態、類型標籤、主要平台、粉絲總數、驗證狀態。
- KOL 自我推薦：KOL 針對合作或案件撰寫的介紹。
- 基本資料：基本識別、聯絡摘要、KOL 定位。
- 社群頻道：平台、帳號、粉絲數、驗證狀態、外部連結。
- 合作偏好：可合作條件、是否接受導購分潤。
- 合作紀錄：過往合作案件、金額、日期、驗收結果、導購成效或成果驗收連結。
- 固定操作：關閉、拒絕報名、接受報名。
- 評分詳情 modal：綜合評分與各評分維度。

**查詢資料**

- `GetMerchantKolProfileQuery`
  - Input：`MerchantId`、`KolId`，若由案件進入需包含 `CaseId` 或 `ApplicationId`。
  - Output：KOL 基本資料、社群帳號、合作偏好、合作紀錄、評分摘要、該案件報名狀態。
  - 資料來源：`KolProfiles`、`KolSocialAccounts`、`KolVerificationRecords`、`CaseApplications`、`Tasks`、`Reviews`、`ReviewScores`。

**提交動作**

- `AcceptKolApplicationCommand`
  - 驗證案件仍可接受 KOL。
  - 驗證剩餘名額。
  - 將報名狀態改為已接受。
  - 建立或綁定案件任務。
  - 寫入操作紀錄。
- `RejectKolApplicationCommand`
  - 將報名狀態改為已拒絕。
  - 保存拒絕原因欄位；若畫面未要求原因可先允許空值。
  - 寫入操作紀錄。

**隱私規則**

- 手機號碼預設遮罩。
- Email 完整顯示。
- 外部社群連結只顯示 KOL 已公開或已驗證的帳號。
- 業者不可透過此頁查詢與自己案件無關且未公開的 KOL 隱私資料。

## MER-010 錢包與充值頁

**頁面目的**

- 讓業者查看錢包可用額度、已鎖定金額、總餘額，並進行帳戶充值與查詢交易紀錄。
- 本頁是案件發布鎖款、案件結案撥款、退款與充值的共同入口，所有金額異動必須走錢包交易流水；此處金額與交易流水皆為平台示意帳務與對帳依據，不代表系統直接執行實際收款或匯款。

**主要區塊**

- 錢包摘要：可用餘額、已鎖定金額、總餘額。
- 帳戶儲值：儲值金額、支付方式、統一編號、電子信箱、送出充值。
- 最近交易記錄：交易時間、交易類型、金額、狀態、關聯案件。
- 交易紀錄明細：日期範圍、交易類型、交易狀態、關鍵字搜尋、交易列表。
- 輔助資訊：電子錢包安全性、客服聯絡。

**查詢資料**

- `GetMerchantWalletQuery`
  - Input：`MerchantId`。
  - Output：可用餘額、已鎖定金額、總餘額、最近交易紀錄、可用支付方式。
  - 資料來源：`MerchantWallets`、`MerchantWalletTransactions`、`Cases`。
- `GetMerchantWalletTransactionsQuery`
  - Input：`MerchantId`、dateRange、transactionType、status、keyword、page、pageSize。
  - Output：交易紀錄明細、分頁資訊、查詢條件。
  - 關鍵字可查案件名稱、案件編號或交易 ID。

**提交動作**

- `CreateMerchantRechargeCommand`
  - 驗證儲值金額大於 0。
  - 第一版支付方式只支援銀行轉帳/ATM。
  - 驗證統一編號、電子信箱格式；兩者用途為付款通知與帳務對帳，不作為發票開立規則。
  - 建立充值交易，狀態先為處理中或待付款。
  - 不建立第三方金流訂單；需保存人工對帳所需的轉帳資訊或備註。
  - 寫入操作紀錄。
- `ConfirmMerchantRechargeCommand`
  - 由後台或人工對帳確認付款後呼叫。
  - 將充值交易轉為已完成。
  - 增加可用餘額。
  - 必須具備防重複確認機制，避免同一筆充值重複入帳。

**交易類型**

| 類型 | 說明 | 對餘額影響 |
| ---- | ---- | ---------- |
| 帳戶充值 `Recharge` | 業者儲值入錢包 | 增加可用餘額 |
| 專案預算凍結 `Lock` | 案件發布或追加預算時鎖定金額 | 減少可用餘額，增加已鎖定金額 |
| 專案結算 `Settlement` | 任務驗收與帳務完成後撥款或扣款 | 減少已鎖定金額 |
| 退款 `Refund` | 取消案件、釋放未使用預算或其他退款 | 增加可用餘額或釋放已鎖定金額 |
| 案件結案撥款 `Payout` | 結案時支付 KOL 或平台費用 | 減少已鎖定金額 |

**交易狀態**

- 待付款：充值尚未完成付款。
- 處理中：交易已建立但尚未完成入帳、鎖款、退款或撥款。
- 已完成：金額異動已完成且反映於錢包餘額。
- 已取消：交易取消，不應影響最終餘額。
- 失敗：交易失敗，不應影響最終餘額；若已暫扣需有補償交易。

**餘額規則**

- 可用餘額：可立即用於發布案件或追加預算的金額。
- 已鎖定金額：已被案件預算、待執行任務或結算流程占用，但尚未完成最終撥款或釋放的金額。
- 總餘額：預設為可用餘額加已鎖定金額；若未來加入爭議凍結、退款中等子狀態，公式需再擴充。
- 任何加值、鎖定、結算、退款與撥款都必須新增 `MerchantWalletTransactions`，不得只直接覆寫 `MerchantWallets` 餘額欄位。
- `MerchantWalletTransactions` 是示意帳務流水與財務對帳依據；實際收款、退款與匯款由管理者/財務線下處理。
- 交易列表的餘額欄代表該筆交易完成後的錢包餘額快照，便於稽核與對帳。

**權限與稽核**

- 查看錢包需 `Merchant.Wallet.View`。
- 建立充值需 `Merchant.Wallet.TopUp`。
- 查詢交易明細需 `Merchant.Wallet.Transaction.View`。
- 充值建立、人工確認入帳、鎖款、退款、結算與撥款都必須寫入操作紀錄或系統紀錄。

## MER-014 導購成效總覽

**頁面目的**

- 讓業者快速查看導購交易成效、可結算分潤、退款取消金額與有效 KOL 數。
- 本頁是導購儀表板，資料以業者名下啟用導購分潤的案件與 KOL 交易為範圍。

**主要區塊**

- 時間快捷篩選：本月、上月、近 3 個月、今年、自訂區間。
- KPI 摘要：導購交易數、訂單總金額、可結算分潤、退款/取消金額、本月訂單量額、有效 KOL 數。
- 本期導購趨勢：訂單總金額與可結算分潤折線圖。
- 熱門案件 Top 5：排名、案件名稱、交易數、訂單總金額、可結算分潤、查看明細。
- 高效 KOL Top 5：排名、KOL 名稱、主要平台、交易數、訂單總金額、查看明細。
- 頂部操作：查看交易明細、重新同步資料；匯出統計 CSV 第一版暫緩，可在 UI 保留停用/隱藏占位。

**查詢資料**

- `GetMerchantReferralOverviewQuery`
  - Input：`MerchantId`、dateRange。
  - Output：KPI 摘要、趨勢資料、熱門案件排行、高效 KOL 排行。
  - 資料來源：`Cases`、`CaseApplications`、`Tasks`、`ReferralCampaigns`、`ReferralLinks`、`ReferralOrders`、`KolProfiles`。
- 匯出統計 CSV 第一版暫緩，不建立 `ExportMerchantReferralOverviewCommand`，不產生檔案。

**提交動作**

- `ReceiveReferralDataImportCommand`
  - 外部系統定期將導購訂單資料傳入本系統資料庫。
  - 需記錄外部同步批次、接收時間、處理結果與錯誤訊息。
  - 匯入結果需寫入系統操作紀錄。

**統計規則**

- 導購交易數：符合篩選期間且歸屬於該業者導購案件的有效訂單數。
- 訂單總金額：符合篩選期間的有效訂單金額加總，需扣除退款、取消與異常交易。
- 可結算分潤：已達可結算條件的分潤金額，不等同預估分潤。
- 退款/取消金額：退款、取消、作廢訂單金額加總。
- 有效 KOL 數：篩選期間內導購金額大於 0 的 KOL 數。

**權限**

- 查看總覽需 `Merchant.Referral.View`。
- `Merchant.Referral.Export` 可保留為未來擴充權限，第一版不啟用。
- 導購資料由外部系統定期匯入；業者端若保留重新整理按鈕，只重新讀取本系統資料庫，不直接觸發外部同步。

## MER-016 導購明細頁

**頁面目的**

- 讓業者查詢每筆導購訂單、交易狀態、可結算時間與異常原因。
- 本頁資料來源可能來自外部訂房系統或導購預定系統，後端需有可稽核的同步紀錄。

**主要區塊**

- KPI 摘要：導購總交易數、訂單總金額、可結算收益、異常交易數。
- 篩選條件：案件名稱、KOL 合作對象、來源渠道、交易狀態、Checkout 退房時間範圍。
- 訂單列表：訂房訂單編號、案件、KOL、使用折扣碼、使用 CID、訂單總金額、退房完成時間、可結算時間、交易狀態、操作。
- 操作：查看明細/訂單、查看異常原因。
- 匯出報表第一版暫緩，可在 UI 保留停用/隱藏占位。

**查詢資料**

- `GetMerchantReferralOrdersQuery`
  - Input：`MerchantId`、caseId、kolId、source、status、checkoutDateRange、keyword、page、pageSize。
  - Output：KPI 摘要、訂單列表、分頁資訊。
  - 資料來源：`ReferralOrders`、`ReferralOrderAttributions`、`ReferralCampaigns`、`ReferralLinks`、`Cases`、`KolProfiles`。
- `GetMerchantReferralOrderDetailQuery`
  - Input：`MerchantId`、`ReferralOrderId`。
  - Output：訂單明細、歸因資訊、分潤計算、同步紀錄、異常原因。

**提交動作**

- 匯出明細報表第一版暫緩，不建立 `ExportMerchantReferralOrdersCommand`，不產生檔案。

**交易狀態**

- 可結算：訂單已符合分潤結算條件。
- 處理中：訂單尚未達可結算條件，例如尚未退房或仍在等待外部系統確認。
- 異常交易：同步資料缺漏、歸因衝突、訂單退款取消或外部狀態不一致。
- 已取消：訂單取消，不應計入可結算收益。

**外部系統規則**

- 「查看明細/訂單」提供只讀外部連結。
- 後端需提供安全導向 URL，不可讓前端拼接敏感連結。
- 外部訂單同步必須保留原始外部訂單 ID、來源系統、同步時間、同步批次與原始狀態。

**權限與稽核**

- 查看明細需 `Merchant.Referral.Order.View`。
- `Merchant.Referral.Order.Export` 可保留為未來擴充權限，第一版不啟用。
- 查詢不寫操作紀錄；重新同步、外部導向需寫入操作紀錄；匯出功能第一版暫緩。

## MER-008 設定中心

**頁面目的**

- 作為業者端設定入口，集中管理帳戶權限、企業資料、通知偏好與系統輔助資訊。

**主要區塊**

- 設定入口卡：
  - 使用者與角色設定。
  - 使用者角度與權限設定。
  - 企業資料維護。
  - 通知偏好設定。
- 系統維護資訊：上次設定更新時間、說明文件、客服支援。
- 頁尾文件：隱私權政策、服務條款、安全報告。

**查詢資料**

- `GetMerchantSettingsHomeQuery`
  - Input：`MerchantId`、`UserId`。
  - Output：可用設定入口、上次設定更新時間、文件連結、客服資訊。
  - 資料來源：`Merchants`、`MerchantMembers`、`Roles`、`RolePermissions`、`MerchantSettingsAuditLogs` 或 `ActivityLogs`。

**入口權限**

- 使用者與角色設定：成員管理入口，需 `Merchant.UserRole.View`。
- 使用者角度與權限設定：角色權限矩陣入口，需 `Merchant.Permission.View`。
- 企業資料維護：需 `Merchant.Profile.View`。
- 通知偏好設定：需 `Merchant.NotificationPreference.View`。
- 無權限的入口應隱藏或 disabled，避免進入後才報錯。

**入口規則**

- 「使用者與角色設定」進入成員列表與邀請管理。
- 「使用者角度與權限設定」進入角色權限矩陣。

## MER-018 企業資料維護頁

**頁面目的**

- 讓業者維護公司基本資料、識別資料與聯絡窗口。
- 此頁偏向業者自主管理資料；若涉及審核中的敏感欄位，需和後台 ADM-003/ADM-004 的資料規則一致。

**主要區塊**

- 公司基本資訊：公司/法人名稱、英文/商業名稱、統一編號、公司電話、傳真、公司信箱。
- 聯絡窗口清單：多筆聯絡人，可新增、編輯、移除。
- 聯絡窗口欄位：姓名、電話、分機、手機、電子信箱、傳真、聯絡時段或備註。
- 操作：查看異動紀錄、取消、儲存變更。

**查詢資料**

- `GetMerchantCompanyProfileQuery`
  - Input：`MerchantId`。
  - Output：公司基本資料、聯絡窗口清單、最後更新資訊。
  - 資料來源：`Merchants`、`MerchantProfiles`、`MerchantContacts`、`ActivityLogs`。

**提交動作**

- `UpdateMerchantCompanyProfileCommand`
  - 更新公司基本資料與識別資訊。
  - 驗證必填欄位與格式。
  - 寫入操作紀錄。
- `UpsertMerchantContactCommand`
  - 新增或更新聯絡窗口。
  - 至少需保存姓名、電話或手機、Email。
  - 寫入操作紀錄。
- `RemoveMerchantContactCommand`
  - 移除聯絡窗口。
  - 建議採軟刪除，保留歷史操作紀錄。

**欄位規則**

- 必填欄位建議沿用 ADM-004：公司/法人名稱、統一編號、公司電話、公司信箱。
- 統一編號需檢查格式，但是否需驗證工商資料待確認。
- Email 需檢查格式。
- 聯絡窗口至少需保留一筆主要聯絡人，不可刪到 0 筆。

**權限與稽核**

- 查看企業資料需 `Merchant.Profile.View`。
- 儲存企業資料需 `Merchant.Profile.Edit`。
- 查看異動紀錄需 `Merchant.Profile.Audit.View`。
- 公司資料與聯絡窗口異動都需寫入操作紀錄，內容至少包含操作者、異動欄位、舊值、新值、時間與 IP。

## MER-012 通知偏好設定頁

**頁面目的**

- 讓業者設定系統通知通道與各事件類型的通知方式，避免重要資訊漏接。

**主要區塊**

- 全域通知管道：Email 通知、站內訊息。
- 事件觸發矩陣：事件類型 x 通知管道。
- 事件類型：
  - 待驗收：合作 KOL 已提交成果，等待確認。
  - 名額未滿：任務截止前 24 小時投稿人數不足。
  - 案件審核：提交的新任務需通過審查。
  - 系統維護公告：停機或更新說明。
  - 錢包餘額變動：儲值成功、扣款或撥款通知。
- 操作：還原預設、儲存設定。

**查詢資料**

- `GetMerchantNotificationPreferencesQuery`
  - Input：`MerchantId`、`UserId`。
  - Output：全域通道設定、事件通知矩陣、不可關閉的強制通知清單。
  - 資料來源：通用 `NotificationPreferences`、`MerchantMembers`；業者端使用 `OwnerType = Merchant` 與 `MerchantId` 儲存公司層級通知偏好。

**提交動作**

- `UpdateMerchantNotificationPreferencesCommand`
  - 更新全域通道與事件矩陣。
  - 驗證不可關閉的緊急通知仍保持啟用。
  - 寫入操作紀錄。
- `ResetMerchantNotificationPreferencesCommand`
  - 還原系統預設通知設定。
  - 寫入操作紀錄。

**通知規則**

- 通知偏好採公司層級設定；同一業者內成員共用 `OwnerType = Merchant` 的偏好設定。
- Email 通知需驗證成員 Email 可用。
- 站內訊息需建立 `Notifications` 或等價資料，供右上角通知中心讀取。
- 強制通知不可關閉，第一版包含安全、付款失敗、系統維護。
- 若未來要支援成員個人覆寫，可再使用 `OwnerType = User` 增加使用者層級偏好。

**權限與稽核**

- 查看通知偏好需 `Merchant.NotificationPreference.View`。
- 儲存通知偏好需 `Merchant.NotificationPreference.Edit`。
- 還原預設需 `Merchant.NotificationPreference.Edit`。
- 儲存與還原都需寫入操作紀錄。

## MER-011 使用者與權限管理

**頁面目的**

- 讓業者管理公司成員、角色、帳號狀態與邀請流程。
- 系統只能有一位 Owner，確保帳號管理責任清楚。

**主要區塊**

- 成員列表：姓名、電子郵件、角色、狀態、加入時間、操作。
- 搜尋與篩選：姓名、Email、角色關鍵字、角色下拉。
- 操作：新增成員、編輯、權限轉移、停用、重新寄送邀請、刪除。
- 帳號安全提醒：提醒定期審查成員清單與移除不需要權限的離職員工。
- 新增成員頁：姓名、電子郵件、預設角色設定、發送邀請。
- 邀請說明：受邀者以 Email 收到邀請連結，需於 48 小時內完成帳號開通。

**查詢資料**

- `GetMerchantMemberListQuery`
  - Input：`MerchantId`、keyword、roleId、status、page、pageSize。
  - Output：成員列表、角色選項、分頁資訊。
  - 資料來源：`Users`、`MerchantMembers`、`Roles`、`UserInvitations`。
- `GetMerchantMemberInvitePageQuery`
  - Input：`MerchantId`、`UserId`。
  - Output：可指派角色、郵件服務狀態。

**提交動作**

- `InviteMerchantMemberCommand`
  - 驗證姓名與 Email 必填。
  - 驗證 Email 尚未是該業者有效成員。
  - 第一版不限制席位數。
  - 建立 `UserInvitations`，預設 48 小時有效。
  - 發送邀請信。
  - 寫入操作紀錄。
- `UpdateMerchantMemberCommand`
  - 更新成員名稱、角色或其他可編輯資料。
  - Owner 只能有一位；Owner 轉移需透過 `TransferMerchantOwnerCommand`。
  - 寫入操作紀錄。
- `TransferMerchantOwnerCommand`
  - 將 Owner 權限轉移給其他成員。
  - 需驗證操作者具備 Owner 權限。
  - 寫入高風險操作紀錄。
- `DisableMerchantMemberCommand`
  - 停用成員登入與業者端操作權限。
  - 不刪除歷史操作紀錄。
- `ResendMerchantInvitationCommand`
  - 針對待確認邀請重新寄送邀請信。
  - 取消舊邀請並建立新邀請，避免多個有效 token 並存。
- `DeleteMerchantInvitationCommand`
  - 刪除或取消尚未接受的邀請。
  - 已啟用成員不可硬刪除，應使用停用。

**角色與狀態**

- 角色示例：Owner、Admin、Member。
- 狀態示例：
  - 已啟用：可登入並依角色權限操作。
  - 待確認：邀請已送出但尚未完成註冊或接受。
  - 停用：不可登入業者端。
- Owner 具有全部權限；Admin 可管理部分設定；Member 僅能操作被授權的業務功能。
- Owner/Admin/Member 採固定三角色權限矩陣；此矩陣作為第一版 seed data 預設值，後續可依營運需求調整或由業者自訂角色覆蓋。
- 實際角色名稱與權限矩陣需和後台角色/權限規格共用，不應在業者端另建一套不可對應的權限模型。

**預設角色權限矩陣**

| 功能 | 建議 Permission | Owner | Admin | Member |
| ---- | --------------- | ----- | ----- | ------ |
| 查看首頁 | `Merchant.Home.View` | 是 | 是 | 是 |
| 查看案件 | `Merchant.Case.View` | 是 | 是 | 是 |
| 新增 / 編輯案件 | `Merchant.Case.Manage` | 是 | 是 | 否 |
| 發布案件 / 鎖定預算 | `Merchant.Case.Publish` | 是 | 是 | 否 |
| 取消案件 | `Merchant.Case.Cancel` | 是 | 是 | 否 |
| 接受 / 拒絕 KOL 報名 | `Merchant.Application.Manage` | 是 | 是 | 否 |
| 查看 KOL 名單 / 資料 | `Merchant.Kol.View` | 是 | 是 | 是 |
| 驗收成果 | `Merchant.Task.Review` | 是 | 是 | 否 |
| 查看導購成效 | `Merchant.Referral.View` | 是 | 是 | 是 |
| 匯出導購 / 報表 | `Merchant.Referral.Export` | 暫緩 | 暫緩 | 暫緩 |
| 查看導購訂單明細 | `Merchant.Referral.Order.View` | 是 | 是 | 是 |
| 匯出導購訂單明細 | `Merchant.Referral.Order.Export` | 暫緩 | 暫緩 | 暫緩 |
| 查看錢包 | `Merchant.Wallet.View` | 是 | 是 | 否 |
| 充值 | `Merchant.Wallet.TopUp` | 是 | 是 | 否 |
| 查看交易明細 | `Merchant.Wallet.Transaction.View` | 是 | 是 | 否 |
| 查看企業資料 | `Merchant.Profile.View` | 是 | 是 | 是 |
| 編輯企業資料 | `Merchant.Profile.Edit` | 是 | 是 | 否 |
| 查看企業資料異動紀錄 | `Merchant.Profile.Audit.View` | 是 | 是 | 否 |
| 查看通知偏好 | `Merchant.NotificationPreference.View` | 是 | 是 | 否 |
| 儲存 / 還原通知偏好 | `Merchant.NotificationPreference.Edit` | 是 | 是 | 否 |
| 查看成員列表 | `Merchant.UserRole.View` | 是 | 是 | 否 |
| 邀請成員 | `Merchant.UserRole.Invite` | 是 | 是 | 否 |
| 編輯成員 | `Merchant.UserRole.Edit` | 是 | 是 | 否 |
| 停用成員 | `Merchant.UserRole.Disable` | 是 | 是 | 否 |
| Owner 轉移 | `Merchant.UserRole.TransferOwner` | 是 | 否 | 否 |
| 查看角色權限矩陣 | `Merchant.Permission.View` | 是 | 否 | 否 |
| 新增 / 編輯角色與權限 | `Merchant.Permission.Manage` | 是 | 否 | 否 |

- Owner 是業者端最高權限角色，包含帳號、角色、財務、案件、企業資料與通知偏好。
- Admin 是業者端營運管理角色，可管理案件、KOL、錢包充值、企業資料與通知偏好，但不可轉移 Owner 或管理角色權限。
- Member 是業者端一般成員，預設只可查看營運資料，不可操作錢包、發布、驗收、成員與角色權限。

**角色權限管理**

- 角色頁籤：Owner、Admin、Member，以及自訂新增角色。
- 角色資訊卡：角色名稱、角色說明、核心職責、系統保留提示。
- 權限矩陣群組：
  - 案件管理 `Case Management`：新增/編輯案件、發布案件、刪除案件、查看報名名單、驗收成果。
  - 財務結算 `Financials`：查看錢包、儲值、查看交易明細；下載報表/匯出第一版暫緩，可保留為未來權限項。
  - 系統設定 `Settings`：編輯成員權限、新增/移除成員、通知設定。
- 每個群組支援全選；每個權限項目可單獨勾選。
- Owner 預設為系統最高權限角色，通常不可移除重大管理入口。
- 系統保留角色不可改名，但可修改描述。

**角色權限查詢**

- `GetMerchantRolePermissionMatrixQuery`
  - Input：`MerchantId`、`RoleId`。
  - Output：角色資訊、權限群組、權限項目、目前勾選狀態、是否系統保留角色。
  - 資料來源：`Roles`、`Permissions`、`RolePermissions`、`MerchantMembers`。

**角色權限提交動作**

- `CreateMerchantRoleCommand`
  - 新增自訂角色。
  - 驗證角色名稱必填且不得和同業者既有角色重複。
  - 寫入操作紀錄。
- `UpdateMerchantRoleCommand`
  - 更新角色名稱與角色描述。
  - 系統保留角色不可改名，但可修改描述。
  - 寫入操作紀錄。
- `UpdateMerchantRolePermissionsCommand`
  - 更新角色權限矩陣。
  - 若角色已有使用中成員，權限異動需立即套用，並讓既有 session 權限失效重載。
  - 不可移除最後一位 Owner 的必要管理權限。
  - 寫入高風險操作紀錄。

**資料表建議**

- `MerchantMembers`：業者與使用者關聯、角色、狀態、加入時間。
- `Roles` / `RolePermissions`：角色與權限。
- `UserInvitations`：邀請 Email、邀請 token、角色、期限、狀態。
- `ActivityLogs` 或 `MerchantSettingsAuditLogs`：成員與權限異動紀錄。

**權限與稽核**

- 查看成員需 `Merchant.UserRole.View`。
- 邀請成員需 `Merchant.UserRole.Invite`。
- 編輯成員需 `Merchant.UserRole.Edit`。
- 停用成員需 `Merchant.UserRole.Disable`。
- 權限轉移需 `Merchant.UserRole.TransferOwner`。
- 邀請、角色異動、停用、權限轉移、刪除邀請都必須寫入操作紀錄。

## MER-018-A 業者登入頁

> Figma 目前同時使用 `MER-018` 表示「企業資料維護」與「業者登入頁」。後端文件先以 `MER-018-A` 標記登入頁，待 PM 修正頁面代號。

**頁面目的**

- 讓業者成員以公司識別、統一編號、帳號 Email 與密碼登入業者端。
- 支援記住我、忘記密碼與立即申請註冊入口。

**主要區塊**

- 登入表單：公司識別、公司統一編號、帳號/電子郵件、密碼。
- 輔助操作：記住我、忘記密碼、立即申請。
- 頁尾文件：Privacy Policy、Terms of Service、Help Center。

**提交動作**

- `MerchantLoginCommand`
  - 驗證公司識別與統一編號可對應有效 `Merchants`。
  - 驗證帳號屬於該業者有效 `MerchantMembers`。
  - 驗證密碼與帳號狀態。
  - 建立登入 session 或 token。
  - 若勾選記住我，延長 refresh token 或 cookie 有效期限。
  - 寫入登入使用紀錄。

**安全規則**

- 登入失敗需使用通用錯誤訊息，不可揭露公司識別、統編或 Email 哪一項不存在。
- 需有登入失敗次數限制、鎖定或驗證碼機制，詳細規則待確認。
- 停用成員、停用業者、待確認邀請不可登入。

## MER-019 申請註冊流程

**頁面目的**

- 讓新業者提交註冊申請，並透過 Email 驗證完成帳號啟用。

**主要區塊**

- 申請註冊表單：帳號登入憑證為電子郵件地址、密碼、確認密碼；業者名稱、統一編號、聯絡人姓名等為業者公司/申請資料。
- 同意條款：服務條款與隱私權政策。
- 註冊成功頁：提示已寄送驗證信，提供回到登入頁。

**提交動作**

- `SubmitMerchantRegistrationCommand`
  - 驗證必填欄位。
  - 驗證統一編號與 Email 是否已存在。
  - 驗證密碼與確認密碼一致。
  - 建立待驗證業者申請、初始 Owner 成員與驗證 token。
  - 發送 Email 驗證信。
  - 寫入系統紀錄。
- `VerifyMerchantRegistrationEmailCommand`
  - 驗證 Email token。
  - 啟用業者帳號與初始 Owner 成員。
  - 寫入使用紀錄。

**資料表建議**

- `MerchantRegistrationRequests`：申請資料、狀態、來源 IP、送出時間。
- `Merchants`：業者主檔，可先建立待驗證或待審核狀態。
- `Users` / `MerchantMembers`：初始 Owner 帳號。
- `EmailVerificationTokens` 或通用 `UserTokens`：Email 驗證 token。

**狀態規則**

- 註冊送出後需完成 Email 驗證；Email 驗證完成後直接啟用業者帳號。
- 業者端帳號註冊的登入憑證最低欄位為 Email + 密碼；公司資料欄位仍依申請流程另外保存。
- 若驗證信未收到，需提供重寄驗證信頁面。

## MER-020 忘記密碼頁

**頁面目的**

- 讓業者成員以公司統一編號與 Email 申請密碼重設連結。

**主要區塊**

- 忘記密碼表單：公司統一編號、電子郵件。
- 操作：寄送重設連結、返回登入頁。
- 頁首/頁尾：客服支援、隱私權政策、服務條款、說明中心。

**提交動作**

- `RequestMerchantPasswordResetCommand`
  - 接收公司統一編號與 Email。
  - 若對應有效業者成員，建立一次性密碼重設 token 並寄信。
  - 不論是否存在帳號，前端回應應保持一致，避免帳號枚舉。
  - 寫入安全事件紀錄。
- `ResetMerchantPasswordCommand`
  - 以重設 token 設定新密碼。
  - 驗證 token 尚未過期且未使用。
  - 可計算並回傳密碼強度/複雜度提示，但不得因強度不足阻擋送出；僅檢查密碼有填寫、密碼與確認密碼一致，並依使用者輸入設定密碼。
  - 更新密碼、使既有 session 失效，並寫入安全事件紀錄。

**缺頁提醒**

- 目前 Figma 只看到「寄送重設連結」頁，尚未看到「輸入新密碼」與「重設完成」頁；後端仍需保留對應 Command 與 token 流程。
