# KOL Figma Page Specs

> This file is referenced by `.github/CONTRIBUTING.md`. It contains KOL-side Figma page specs translated into backend implementation notes.

## KOL-001 LINE LIFF 入口 / 任務導流頁

**頁面目的**

- 作為 KOL 端進入任務系統的首頁與導覽入口。
- 顯示 KOL 基本狀態、資料完成度、審核狀態，並導向案件、我的任務、收益、我的資料。
- KOL 端預計在 LINE LIFF 中運作；LINE Login/OAuth 方向已在 MEMORY 定案，但 `UserExternalLogins` 尚未進 schema。

**主要區塊**

- 頁首：首頁標題、選單、個人圖示。
- KOL 摘要卡：頭像、名稱、資料完成度、審核狀態、LINE 標示。
- 探索新商機：查看可報名任務、註冊/填寫資料。
- 快速入駐說明：未完成資料或審核前仍可瀏覽可報名任務，但需完成資料與審核才可接案。
- 底部導覽：案件、我的任務、收益、我的。
- 側邊 MENU：案件、我的任務、收益、我的。

**查詢資料**

- `GetKolHomeQuery`
  - Input：`UserId`。
  - Output：KOL 摘要、資料完成度、審核狀態、LINE 綁定狀態、導覽權限。
  - 資料來源：`Users`、`KolProfiles`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、`UserExternalLogins`（待補）。

**LINE / LIFF 規則**

- KOL 端使用 LINE Login OAuth 取得 LINE `userId`，不可和 `KolProfiles.LineContactId` 混用。
- `KolProfiles.LineContactId` 定位為聯絡用 LINE ID；OAuth 綁定需使用 `UserExternalLogins.ProviderUserId`。
- LINE Notify 已停止服務；主動推播需使用 LINE Messaging API。
- 若 LINE Login 不提供 Email，首次登入需強制補填 Email 後才能建立或完成 `Users` 帳號；`Users.Email` 維持必填且全系統唯一。

**權限與狀態**

- 未審核 KOL 可瀏覽案件，但報名/接案需依 KOL 審核狀態限制。
- KOL 停權需同步 `KolProfiles.VerificationStatus = Suspended` 與 `Users.Status = Suspended`。
- 首頁查詢不寫操作紀錄。

## KOL-006 任務清單頁

**頁面目的**

- 讓 KOL 搜尋與篩選可報名案件，查看推薦任務並進入任務詳情。

**主要區塊**

- 頁首：任務管理、選單、個人圖示。
- 提示訊息：需完成資料建立與審核通過後，才可被業者選定接案。
- 搜尋：任務關鍵字。
- 獎勵類型篩選：全部、體驗回饋、現金酬勞、導購分潤。
- 地區與平台篩選。
- 推薦任務卡：案件名稱、評分、截止日期、描述、地點、平台、預估回饋、查看詳情。

**查詢資料**

- `GetKolAvailableCasesQuery`
  - Input：`KolId`、keyword、rewardTypes、region、platforms、page、pageSize。
  - Output：可報名案件清單、推薦標記、KOL 是否符合條件、已報名狀態。
  - 資料來源：`Cases`、`Merchants`、`CasePlatforms`、`CaseCategories`、`CaseRequirements`、`CaseBarterItems`、`CaseBudgetSnapshots`、`CaseApplications`、`KolProfiles`、`KolCategories`、`KolSocialAccounts`。

**篩選與排序規則**

- 只顯示可對 KOL 開放的案件，至少需排除 `Draft`、`Settled`、`Cancelled`。
- `CaseRequirements` 在現有 schema 註解中為提示，不阻擋報名；本頁需顯示「符合/不符合」與原因，但仍允許 KOL 報名。
- 推薦任務排序：匹配度優先，其次截止日期，再其次更新時間。

**操作**

- 查看詳情：進入 `KOL-007`。
- 若 KOL 已報名同案件，列表需顯示目前 `CaseApplications.Status`，不可重複報名。

## KOL-007 案件詳情頁

**頁面目的**

- 讓 KOL 查看單一案件完整資訊、合作條件、交付要求、附件，並決定是否報名。

**主要區塊**

- 案件摘要：案件名稱、業者、地區、截止報名日期、目前報名狀態。
- 資格提示：符合報名資格或顯示不符合原因。
- 任務說明：案件描述。
- 任務酬勞：體驗項目、現金酬勞、導購分潤。
- 發布平台：IG、FB、TikTok 等。
- 任務日期：報名截止、交稿截止。
- 附件檔案：可下載素材、合約或可預覽文件。
- 交付要求：Reels、貼文、影片、Hashtag、授權期間等。
- 業者評分詳情 modal：綜合評分與各評分維度。
- 操作：返回列表、我要報名。

**查詢資料**

- `GetKolCaseDetailQuery`
  - Input：`KolId`、`CaseId`。
  - Output：案件主檔、業者摘要、評分摘要、資格提示、合作條件、平台、日期、附件、交付要求、KOL 報名狀態。
  - 資料來源：`Cases`、`Merchants`、`Reviews`、`ReviewScores`、`CasePlatforms`、`CaseCategories`、`CaseLanguages`、`CaseRequirements`、`CaseBarterItems`、`CaseAttachments`、`Files`、`CaseApplications`。

**狀態規則**

- 尚未報名：顯示「我要報名」。
- 已報名：顯示已報名或等待業者確認。
- 已接受：不可再次報名，需導向我的任務。
- `PendingReconfirmation`：需顯示重新確認入口，對應案件修改後 KOL 是否仍願意參與。
- 已拒絕、已取消、Invalid：不可再次報名或需依 PM 規則決定是否重新開放。

**操作**

- `ApplyCaseCommand`
  - 驗證案件可報名。
  - 驗證同一案件同一 KOL 不得重複報名。
  - 保存 KOL 自我推薦訊息。
  - 依目前條件計算 `IsRequirementMatched` 與 `MismatchReasons`。
  - 建立 `CaseApplications`，初始狀態為 `Applied`。
  - 寫入操作紀錄與通知業者。

## KOL-008 任務報名表

**頁面目的**

- 讓 KOL 填寫自我推薦內容並送出案件報名。

**主要區塊**

- 案件摘要：案件名稱、截止日期、交付成品、任務獎勵。
- KOL 自我推薦：必填文字，建議字數 200～500 字。
- 操作：回任務明細、送出報名。

**提交動作**

- `SubmitKolApplicationCommand`
  - Input：`KolId`、`CaseId`、message。
  - 驗證 message 必填與字數限制。
  - 呼叫或等價於 `ApplyCaseCommand`。
    - 成功後導向我的任務/報名紀錄。

**資料規則**

- 自我推薦寫入 `CaseApplications.Message`。
- 同一案件同一 KOL 只能有一筆 `CaseApplications`。
- 若 KOL 條件不符合，現有 schema 支援 `IsRequirementMatched = false` 與 `MismatchReasons`；第一版不阻擋報名，但需保留原因供業者/Admin 判斷。

## KOL-009 我的任務頁

**頁面目的**

- 讓 KOL 查看自己已錄取、執行中、審核中、退回修改、驗收不通過等任務，並依狀態提交成果、重提成果、查看任務、放棄任務或提出申訴。

**主要區塊**

- 頁首：返回、我的任務、通知。
- 狀態分頁：全部、執行中、審核中、退回修改、驗收不通過等。
- 任務卡：業者名稱、案件名稱、任務狀態、截止日期、交付格式、獎勵內容。
- 狀態提示：
  - 執行中：可提交成果、查看任務、放棄任務。
  - 審核中：可查看任務、放棄任務依規則限制。
  - 退回修改：顯示業者回覆，可重新提交。
  - 驗收不通過：顯示業者回覆，可進行申訴。
- 評分業者 modal：驗收完成後對業者評分。

**查詢資料**

- `GetKolTaskListQuery`
  - Input：`KolId`、statusFilter、page、pageSize。
  - Output：任務列表、任務狀態、獎勵摘要、交付格式、最近審核結果。
  - 資料來源：`Tasks`、`Cases`、`Merchants`、`CasePlatforms`、`CaseBarterItems`、`CaseBudgetSnapshots`、`Submissions`、`Reviews`、`Disputes`、`KolEarnings`。

**狀態對應**

- 執行中：`Tasks.Status = InProgress` 或 `PendingExecution`。
- 審核中：最新 `Submissions.Status = Submitted` 或 `Tasks.Status = UnderReview`。
- 退回修改：`Submissions.Status = RevisionRequested` 或 `Tasks.Status = RevisionRequested`。
- 驗收不通過：`Submissions.Status = Rejected` 或 `Tasks.Status = Incomplete`。
- 爭議處理中不新增 `TaskStatus`；以 `Submissions.Status = Disputed` 或未結案 `Disputes` 顯示。

**提交動作**

- `AbandonKolTaskCommand`
  - KOL 主動放棄任務。
  - KOL 執行中也可直接放棄任務，但需留下紀錄，並可能影響評分。
  - 寫入操作紀錄並通知業者。
- `CreateMerchantReviewCommand`
  - KOL 對業者評分。
  - 寫入 `Reviews` / `ReviewScores`。
  - 評分時機為案件結案後，且為必填。

## KOL-010 任務明細頁

**頁面目的**

- 讓 KOL 查看已接任務的完整資訊、導購資訊、交付規範、獎勵內容、任務時程與附件合約。

**主要區塊**

- 任務摘要：業者、案件名稱、任務狀態、截止日期。
- 驗收結果：
  - 尚無驗收結果：提示成果尚未提交或業者尚未完成驗收。
  - 退回修改：顯示業者具體回饋與時間。
  - 驗收不通過：顯示原因與可申訴入口。
- 導購資訊：優惠碼、導購短連結，支援複製。
- 任務描述：案件內容。
- 完整交付規範：平台與件數，例如 IG Reels、FB Post、TikTok。
- 獎勵內容：體驗項目、現金酬勞、導購分潤。
- 任務時程：發文日期、交截止日期。
- 附件與合約：下載素材、預覽合約。
- 操作：返回我的任務、提交成果或重新提交。

**查詢資料**

- `GetKolTaskDetailQuery`
  - Input：`KolId`、`TaskId`。
  - Output：任務摘要、案件資訊、交付規範、導購資訊、獎勵內容、附件、最新提交與驗收結果。
  - 資料來源：`Tasks`、`Cases`、`Merchants`、`CasePlatforms`、`CaseBarterItems`、`CaseBudgetSnapshots`、`CaseAttachments`、`Files`、`Submissions`、`SubmissionItems`、`Disputes`、`ReferralLinks` 或等價導購資料表。

**資料規則**

- KOL 只能查看自己綁定的 `Tasks`。
- 導購資訊需來自案件/任務對應的導購資料，不可前端自行產生優惠碼或短連結。
- 最新驗收結果以最新一筆 `Submissions` 與相關 `Disputes` 推導。

## KOL-011 成果提交頁

**頁面目的**

- 讓 KOL 上傳或填寫任務成果，提交給業者驗收。

**主要區塊**

- 任務摘要：任務編號、案件名稱、截止日期、提交提醒。
- 成果項目清單：可新增多筆成果項目。
- 成果項目欄位：發布平台、成果連結、預覽數、點擊數、截圖附件。
- 提交備註：KOL 補充說明。
- 操作：新增成果項目、刪除項目、上傳附件、送出成果。

**提交動作**

- `SubmitKolTaskResultCommand`
  - Input：`KolId`、`TaskId`、submissionItems、note。
  - 驗證任務屬於 KOL。
  - 驗證任務狀態允許提交或重新提交。
  - 建立新一筆 `Submissions`，狀態為 `Submitted`。
  - 建立多筆 `SubmissionItems`，保存平台、成果連結、附件與備註。
  - 設定 `ReviewDeadlineAt = SubmittedAt + 14 天`；KOL 重提後重算。
  - 將 `Tasks.Status` 更新為 `UnderReview`。
  - 寫入操作紀錄並通知業者。

**資料規則**

- 每次 KOL 提交或重新提交都建立新的 `Submissions`，不得覆寫舊提交。
- 成果提交必填規則依交付規範決定；有連結型成果必填連結，有截圖要求才必填附件，數據欄位依平台與交付規範判斷。
- 附件使用 `Files`，並由 `SubmissionItems.FileId` 關聯。

## KOL-012 申訴詳情頁

**頁面目的**

- 讓 KOL 針對驗收不通過或退回修改結果提出申訴，交由系統管理員介入審核。

**主要區塊**

- 申訴狀態摘要：例如驗收不通過、原因分類。
- 業者驗收意見：業者回覆內容與時間。
- 申訴理由：KOL 必填文字。
- 提醒：申訴案件將由系統管理員介入審核，需提供具體說明。
- 操作：返回、送出申訴。

**提交動作**

- `CreateKolDisputeCommand`
  - Input：`KolId`、`TaskId` 或 `SubmissionId`、reason、description。
  - 驗證任務屬於 KOL。
  - 驗證最新 `Submission.CanDispute()`。
  - 驗證同一任務沒有進行中的 `Disputes`。
  - 建立 `Disputes`，狀態為 `Open`。
  - 將最新 `Submissions.Status` 更新為 `Disputed`。
  - 寫入操作紀錄並通知 Admin/業者。

**狀態規則**

- MEMORY 已定案：爭議不新增 `TaskStatus`，由 `SubmissionStatus.Disputed` 與 `Disputes.Status` 追蹤。
- `Submission.CanDispute()` 目前允許 `Submitted`、`RevisionRequested`、`Overdue`、`Rejected`。
- 驗收不通過不會自動建立異議，需使用者提出申訴才建立 `Disputes`。

## KOL-002 個人資料設定 / 送審頁

**頁面目的**

- 讓 KOL 補齊個人基本資料、創作者定位、合作條件與進階設定，並送出審核。

**主要區塊**

- 完成度摘要：送審狀態、個人資料完成度、缺漏提醒。
- 基礎資料：聯絡人姓名、電話、LINE ID、Email。
- 創作者定位：KOL 類型、自我介紹。
- 合作條件：服務地區、擅長語言、合作模式。
- 進階設定入口：社群頻道設定、收款資料設定、通知設定。
- 送審檢查清單：基礎資料、社群頻道、收款帳戶、平台使用條款。
- 操作：儲存資料、送出審核。

**查詢資料**

- `GetKolProfileSetupQuery`
  - Input：`KolId`。
  - Output：`KolProfiles`、`KolCategories`、合作模式、社群頻道摘要、收款資料摘要、條款同意狀態與完成度。
  - 資料來源：`Users`、`KolProfiles`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、條款同意紀錄或等價資料表。

**提交動作**

- `SaveKolProfileCommand`
- 儲存聯絡人姓名、電話、聯絡用 LINE ID（`LineContactId`）、Email、自我介紹、KOL 類型、合作模式。
  - 服務地區與擅長語言採獨立資料表保存，例如 `KolServiceAreas`、`KolLanguages`。
- `SubmitKolProfileForReviewCommand`
  - 驗證必填：聯絡人姓名、電話、LINE ID、Email、自我介紹、至少一個 KOL 類型、至少一個合作模式、同意平台使用條款。
  - 驗證至少一筆社群頻道；畫面檢查清單標示「社群頻道已連結（需要至少一個）」。
  - 收款帳戶畫面標示選填；不阻擋送審，但提領/收益結算前需強制補齊。
  - 平台使用條款同意先於 `KolProfiles` 記錄同意時間，暫不做條款版本表。
  - 將 `KolProfiles.VerificationStatus` 更新為 `Pending`，並寫入操作紀錄。
  - 若送審前狀態為 `Rejected`，寫入 `KolReviewEvents.ActionType = Resubmitted`；其他首次或一般送審寫入 `KolReviewEvents.ActionType = Submitted`。

**資料表**

- 已存在：`KolProfiles`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、`KolReviewEvents`。
- 待補：`KolServiceAreas`、`KolLanguages`；平台使用條款同意時間可先補在 `KolProfiles`。

**狀態規則**

- `KolProfiles.LineContactId` 是聯絡用 LINE ID，不可當作 LINE OAuth `ProviderUserId`。
- KOL 送審只代表資料進入後台審核流程，不代表自動啟用接案。
- 完成度應由後端依欄位與關聯資料計算，前端不可自行寫死百分比。

## KOL-003 社群頻道設定頁

**頁面目的**

- 讓 KOL 維護社群平台帳號、粉絲數、驗證狀態與是否自動同步數據。

**主要區塊**

- 設定提示：完整設定提升媒合率、立即同步數據。
- 平台卡片：平台、帳號、粉絲數、資料來源、驗證狀態。
- 操作：新增平台、編輯平台、立即同步數據。
- 編輯平台 modal：社群網址、自動同步數據、手動覆蓋粉絲數、取消、儲存變更。

**查詢資料**

- `GetKolSocialChannelsQuery`
  - Input：`KolId`。
  - Output：多筆社群帳號、粉絲數、資料來源、驗證狀態、最後同步時間。
  - 資料來源：`KolSocialAccounts`。

**提交動作**

- `UpsertKolSocialAccountCommand`
  - Input：`KolId`、platform、profileUrl 或 accountName、autoSync、manualFollowersCount。
  - 後端需從社群網址解析平台與帳號；無法解析時回傳驗證錯誤。
  - `autoSync = true` 時設為 `DataSource = ApiSync`；`autoSync = false` 時設為 `ManualInput` 並保存手動粉絲數。
  - 儲存後若需要審核，`VerificationStatus` 預設為 `Unverified` 或 `NeedsConfirmation`。
- `SyncKolSocialAccountCommand`
  - 對支援 API 的平台抓取最新粉絲數與驗證資訊。
  - 更新 `FollowersCount`、`LastSyncAt`、`VerificationStatus`，並寫入操作紀錄。

**資料表**

- 已存在：`KolSocialAccounts`。
- 注意：社群平台以 `schema.sql` 註解的 11 個平台為準（X、IG、FB、YT、Blog、小紅書、TikTok、中國抖音、Threads、Snapchat、WeChat），`Domain.Enums.SocialPlatform` 需補齊。
- 社群資料預計採購外部 API 並以排程同步，每週同步一次。

**狀態規則**

- `DataSource`：`ApiSync` 代表由平台 API 同步；`ManualInput` 代表 KOL 手動輸入。
- `VerificationStatus`：`Verified`、`Unverified`、`NeedsConfirmation`。
- 同一 KOL 同一平台目前 schema 設為唯一；若未來允許同平台多帳號，需調整唯一限制。

## KOL-004 收款資料設定頁

**頁面目的**

- 讓 KOL 維護銀行帳戶與稅務身分資料，供合作費用、稅務申報與二代健保扣繳使用。

**主要區塊**

- 隱私權聲明與資料完整狀態。
- 銀行帳戶資訊：戶名、銀行代碼、銀行名稱、分行名稱/代碼、銀行帳號。
- 稅務身分資訊：
  - 個人（本地）：國籍、姓名、身分證字號、戶籍地址、身分證正反面、存摺影本。
  - 公司 / 工作室：公司名稱、統一編號、登記地址、存摺影本。
- 操作：儲存收款資料、返回我的頁面。

**查詢資料**

- `GetKolPayoutProfileQuery`
  - Input：`KolId`。
  - Output：收款帳戶遮罩資訊、收款資料狀態、稅務身分資料、附件清單。
  - 資料來源：`KolBankAccounts`、`Files`，以及待補的稅務資料表。

**提交動作**

- `SaveKolPayoutProfileCommand`
  - 個人戶必填：戶名、銀行代碼、銀行名稱、分行名稱/代碼、銀行帳號、姓名、身分證字號、戶籍地址。
  - 公司 / 工作室必填：戶名、銀行代碼、銀行名稱、分行名稱/代碼、銀行帳號、公司名稱、統一編號、登記地址。
  - 檔案上傳依身分別保存至 `Files` 並關聯稅務資料。
  - 銀行帳號需加密儲存，回傳給前端時預設遮罩。
  - 儲存後將收款資料狀態設為 `Pending` 或待審核狀態，並寫入操作紀錄。

**資料表**

- 已存在：`KolBankAccounts`，但目前只含 `AccountType`、戶名、銀行代碼、銀行名稱、帳號與狀態。
- 待補：`KolTaxProfiles` 與 `KolTaxDocuments`。`KolBankAccounts` 只管銀行帳戶；稅務身分、國籍類型、身分證字號、戶籍地址、公司名稱、統一編號、登記地址、身分證/存摺附件關聯放入稅務資料表。

**敏感資料規則**

- CONTRIBUTING 已定案：收款資料屬敏感資料，後台與 KOL 端皆需預設遮罩。
- KOL 自己頁面提供眼睛圖示查看完整帳號，只允許本人短時間顯示，並寫入敏感資料查看紀錄。
- 不新增獨立敏感資料查看表；使用擴充後的 `ActivityLogs` 記錄 `TargetType`、`TargetId`、IP、UserAgent 與操作內容。
- 後台永遠遮罩，不顯示完整帳號。

## KOL-005 通知設定頁

**頁面目的**

- 讓 KOL 設定 LINE 與 Email 通知渠道，以及逐事件類型的通知偏好。

**主要區塊**

- 通知渠道：LINE 通知、電子郵件通知。
- 通知事件設定：
  - Selected for task（已被選中參加案件）。
  - Deadline reminder（案件截止提醒）。
  - Rework notice（稿件退回修正）。
  - Review result（案件審核結果）。
  - Sales notice（銷售數據更新）。
  - Earning notice（收益結算通知）。
- 操作：儲存通知設定、還原預設。

**查詢資料**

- `GetKolNotificationPreferencesQuery`
  - Input：`KolId` 或 `UserId`。
  - Output：渠道開關、各事件類型在 LINE / Email 的啟用狀態。
  - 資料來源：通用 `NotificationPreferences`，KOL 使用 `OwnerType = User` 與 `OwnerUserId` 儲存個人通知偏好。

**提交動作**

- `UpdateKolNotificationPreferencesCommand`
  - 儲存 LINE / Email 全域開關與各事件開關。
  - 若 LINE 通知開啟，需確認使用者已有 LINE OAuth / Messaging API 可推播識別，不可使用聯絡用 `KolProfiles.LineContactId` 當推播目標。
- `ResetKolNotificationPreferencesCommand`
  - 還原系統預設通知組合，並寫入操作紀錄。

**資料表**

- 通知偏好採通用 `NotificationPreferences`，不新增 KOL 專用通知偏好表。
- 最低欄位：`OwnerType`、`OwnerUserId`、`EventType`、`Channel`、`IsEnabled`、`IsMandatory`、`CreatedAt`、`UpdatedAt`。
- LINE 推播依 MEMORY 需使用 LINE Messaging API，並依 `UserExternalLogins.ProviderUserId` 或等價欄位定位使用者。

**狀態規則**

- 通知偏好只控制是否發送；事件本身仍需寫入系統內通知或操作紀錄。
- Email 關閉不影響必要法務/安全通知，若有不可關閉的通知需在後續需求標明。

## KOL-014 月結紀錄頁 / 收益結算

**頁面目的**

- 讓 KOL 查看自己的可提領金額、每月結算紀錄與收支明細，並依收款身分完成提領所需文件回傳。

**主要區塊**

- 我的錢包：總金額、提領按鈕、最低提領門檻提醒。
- 月結卡片：結算月份、處理狀態、總金額、結算日期、項目數量。
- 收支明細：日期、案件名稱、收益類型、金額、處理狀態。
- 收款資料不完整提示：導向 `KOL-004` 收款資料設定。
- 個人身分提領流程：下載勞務報酬單、回傳已簽署勞報單、確認送出。
- 公司 / 工作室提領流程：顯示發票抬頭、統一編號、公司地址、聯絡電話，回傳統一發票影本、確認送出。
- 勞務報酬單與發票相關文件模板由財務提供；系統可依財務模板帶入姓名、身分證字號、戶籍/通訊地址、專案名稱、期間、訂單編號、支付金額、扣繳與二代健保等欄位。

**查詢資料**

- `GetKolSettlementOverviewQuery`
  - Input：`KolId`。
  - Output：錢包總額、可提領金額、收款資料完整狀態、近月結算清單。
  - 資料來源：`KolWallets`、`KolEarnings`、`PayoutRequests`、`KolBankAccounts`、月結批次、稅務文件資料表。
- `GetKolMonthlySettlementDetailQuery`
  - Input：`KolId`、settlementMonth。
  - Output：該月總金額、結算狀態、結算日期、收益明細。
  - 資料來源：`KolEarnings`、`Cases`、`Tasks`、`PayoutRequests`、月結批次/結算單資料表。

**提交動作**

- `CreateKolPayoutRequestCommand`
  - 驗證 KOL 收款資料完整且狀態可用。
  - 驗證可提領金額需大於等於系統參數設定的提領門檻；第一版門檻為 NT$1,000。
  - 建立 `PayoutRequests`，狀態為 `Pending`。
  - 將納入本次提領的 `KolEarnings.Status` 從 `Available` 更新為 `Requested`。
  - 寫入操作紀錄並通知 Admin。
- `UploadKolPayoutDocumentCommand`
  - 個人身分：系統產生勞務報酬單，KOL 簽署後回傳。
  - 公司 / 工作室：上傳統一發票影本。
  - 檔案寫入 `Files`，並關聯至本次 `PayoutRequests` 或待補的提領文件資料表。
- `ConfirmKolPayoutDocumentCommand`
  - 驗證本次提領必要文件已上傳。
  - 將提領申請標記為文件已回傳並等待 Admin 審核；Admin 審核通過才可撥款。
  - 寫入操作紀錄並通知 Admin。

**資料表**

- 已存在：`KolWallets`、`KolEarnings`、`PayoutRequests`、`Files`、`KolBankAccounts`。
- 待補：
  - 月結批次或結算單資料表，例如 `KolSettlementBatches` / `KolSettlementStatements`，用來保存結算月份、結算日期、狀態與快照金額。
  - 結算明細表，例如 `KolSettlementItems`，用來固定結算單與 `KolEarnings` 的關聯。
  - 提領文件關聯表，例如 `PayoutRequestDocuments`，用來保存勞報單、發票影本、簽署狀態與上傳檔案。
  - 若要產生勞務報酬單 PDF/影像，需保存財務提供的模板版本與帶入欄位快照，避免日後 KOL 修改個資造成歷史文件變動。

**狀態規則**

- `KolWallets` 是聚合餘額表；金額來源仍以 `KolEarnings` 與提領/結算紀錄為準，不可只覆寫錢包總額。
- 月結卡片的「處理中 / 已完成」需由結算批次或 `PayoutRequests.Status` 推導。
- 已送出提領的收益明細需轉為 `Requested`，避免同一筆收益重複提領。
- Admin 審核通過並完成匯款後，將 `PayoutRequests.Status` 更新為 `Paid`，相關 `KolEarnings.Status` 更新為 `Paid`，並更新 `KolWallets.PaidAmount`。
- 若提領被拒絕或取消，相關 `KolEarnings.Status` 應回到 `Available`，並釋回可提領金額。

**稅務與文件規則**

- 個人戶由系統產生或提供下載勞務報酬單，KOL 簽署後回傳。
- 公司 / 工作室需依平台提供的發票資訊開立統一發票並回傳影本。
- 勞報單與發票相關文件模板由財務提供；實際版型、欄位細節與保存年限待財務模板與法遵規則到位後補齊。
- 勞報單/發票文件需經 Admin 審核通過才可撥款。
- 勞報單/發票影本屬敏感帳務文件，下載、上傳、查看都需檢查本人或 Admin 權限並寫入紀錄。
