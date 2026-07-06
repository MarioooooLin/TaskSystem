# Admin Figma Page Specs

> This file is referenced by `.github/CONTRIBUTING.md`. It contains Admin-side Figma page specs translated into backend implementation notes.

### 14.3 ADM-003 業者詳細檔案頁

**頁面目的**

管理者查看單一業者完整營運資料，並執行停用帳號、代理登入、聯絡窗口維護、折扣金加值/扣回等後台操作。

**主要區塊**

- 業者摘要：公司名稱、狀態、產業/行業、負責人、聯絡信箱、電話、目前餘額、折扣金餘額。
- 基本資料：公司資料、識別資料、公司聯絡資訊。
- 聯絡窗口：多筆窗口卡片，包含主要窗口標記。
- 案件摘要：總案件數、執行中、待審核、已完成、成交率。
- 最近案件：案件編號/名稱、類型、狀態、金額、更新時間。
- 錢包與折扣金：現金錢包、已鎖定金額、可動用餘額、累計折扣金、未使用優惠、當月已用折扣。
- 折扣金加值/扣回操作：類型、金額、適用範圍、有效期限、異動原因、備註。
- 折扣金異動紀錄、折扣金使用紀錄、使用紀錄、操作紀錄。

**查詢資料**

- 使用 `GetMerchantDetailQuery` 類型的查詢取得頁面資料。
- DTO 需包含業者主檔、識別資料、公司聯絡資訊、聯絡窗口、案件摘要、最近案件、錢包摘要、折扣金摘要、折扣金異動紀錄、折扣金使用紀錄、使用紀錄、操作紀錄。
- 現有資料來源優先使用 `Merchants`、`Users`、`MerchantContacts`、`Cases`、`MerchantWallets`、`MerchantWalletTransactions`、`MerchantCreditWallets`、`MerchantCreditTransactions`、`ActivityLogs`。
- 折扣金使用獨立 `MerchantCreditWallets` / `MerchantCreditTransactions`，不併入現金餘額。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 編輯業者資料 | 導向 `ADM-004-EDIT`，不在詳細頁直接更新主檔。 |
| 新增聯絡窗口 | 開啟 `ADM-003-MODAL`，儲存後立即新增 `MerchantContacts`。 |
| 停用業者 | 停用該業者底下全部成員登入帳號，預設更新 `Users.Status = Suspended`。 |
| 代理登入 | 僅保留規格，不在現階段實作。 |
| 折扣金加值/扣回 | 必須建立折扣金交易流水，並在同一交易內更新折扣金餘額。 |

**權限**

- 需具備 Admin 後台權限，建議權限碼：`Admin.Merchant.View`、`Admin.Merchant.Update`、`Admin.Merchant.Suspend`、`Admin.Merchant.Impersonate`、`Admin.Merchant.CreditAdjust`。
- 停用、代理登入、折扣金加值/扣回屬高風險操作，必須寫入操作紀錄。

**狀態規則**

- 停用業者的預設語意是停用登入帳號，而不是只改業者主檔狀態。
- 停用範圍預設包含該 Merchant 底下所有 `MerchantMembers` 對應的 `Users`，避免業者被停用後仍可操作業者端。
- 復權規則尚未從 Figma 定案；實作前需確認是否全部成員復權、僅 Owner 復權，或依停用前狀態還原。
- 代理登入先不實作，但規格需保留：`AdminId`、`TargetUserId`、`MerchantId`、開始時間、結束時間、原因、IP、UserAgent、代理期間操作稽核。

**資料表與稽核**

- 現金錢包沿用 `MerchantWallets` / `MerchantWalletTransactions`。
- 折扣金採獨立 `MerchantCreditWallets` / `MerchantCreditTransactions`，不直接覆寫餘額，不混入現金錢包餘額；折抵規則仍待 PM/財務確認。
- 使用紀錄與操作紀錄預設拆開：
  - 使用紀錄：登入、登出、代理登入、代理登入結束。
  - 操作紀錄：業者資料修改、聯絡窗口新增/修改/刪除、帳號停用、折扣金異動、錢包異動。
- 若現階段不新增專用 log table，至少需寫入 `ActivityLogs`，並保留 `ActorUserId`、`Action`、`BeforeData`、`AfterData`、`Note`、`CreatedAt`。

### 14.4 ADM-004-EDIT 編輯業者資料

**頁面目的**

管理者編輯業者公司基本資料、識別資料與公司聯絡資訊。

**主要區塊與欄位**

| 區塊 | 欄位 |
| ---- | ---- |
| 公司基本資料 | 公司/法人名稱、英文/商業名稱、產業類型、行業類型、啟用狀態 |
| 識別資料 | 統一編號、公司登記地址、建立日期 |
| 公司聯絡資訊 | 公司電話、傳真、公司信箱 |

**提交動作**

- 儲存業者資料時使用 `UpdateMerchantCommand` 類型的 Application Command。
- 更新範圍以 `Merchants` 主檔欄位為主；不得在此頁更新錢包、聯絡窗口、案件狀態或登入密碼。
- 必填欄位固定為公司/法人名稱、統一編號、公司電話、公司信箱。
- 英文/商業名稱、產業類型、行業類型、啟用狀態、公司登記地址、建立日期與傳真皆為選填或系統欄位；建立日期原則上由系統維護，不由此頁人工修改。
- 成功後回到 `ADM-003` 並顯示最新資料；失敗時保留輸入值並回傳欄位錯誤。

**稽核**

- 更新前後差異需寫入操作紀錄。
- 操作紀錄至少包含操作者、MerchantId、異動欄位、異動前後值、時間。

### 14.5 ADM-003-MODAL 新增聯絡窗口

**頁面目的**

管理者在業者詳細頁新增一筆業者聯絡窗口。

**欄位**

| 欄位 | 規則 |
| ---- | ---- |
| 姓名 | 必填。 |
| 公司電話 | 選填；可搭配分機。 |
| 分機 | 選填。 |
| 行動電話 | 選填。 |
| 電子郵件 | 選填；若填寫需符合 email 格式。 |
| 電話 | 選填；作為其他聯絡電話。 |
| 備註 | 選填。 |

**提交動作**

- 按 Modal 的「儲存」即立即寫入 DB，不需等待業者詳細頁另一次儲存。
- 使用 `AddMerchantContactCommand` 類型的 Application Command。
- 寫入 `MerchantContacts`，並回到 `ADM-003` 重新載入聯絡窗口列表。

**稽核**

- 新增聯絡窗口需寫入操作紀錄。
- 操作紀錄至少包含操作者、MerchantId、ContactId、建立內容摘要、時間。

### 14.6 ADM-005 KOL 管理頁

**頁面目的**

管理者搜尋與篩選 KOL 名單，掌握 KOL 啟用狀態、審核狀態、社群狀態、任務紀錄與異議狀況，並可進入詳細資料、任務或異議處理。

**主要區塊**

- 統計摘要：全部 KOL、啟用中、待審核、已退回、停權中、未結案異議。
- 功能按鈕：匯出 KOL 清單、查看待審 KOL。
- 搜尋與篩選：
  - 關鍵字：KOL 名稱、Email、手機、社群帳號。
  - KOL 狀態：全部、啟用中、停權中、待審核、已退回等。
  - KOL 類型：全部、旅遊、美食、親子、寵物、生活風格、美妝、3C、健身、其他。
  - 主要平台：IG、FB、YT、TikTok、Threads、Blog。
  - 收款資料狀態：全部、已填寫、未填寫、待確認等。
  - 社群驗證狀態：全部、已驗證、未驗證、需確認等。
  - 建立日期區間。
- KOL 列表：KOL 名稱、Email、類型/平台、粉絲總數、KOL 狀態、收款資料、任務績效、異議數、建立日期、操作。
- 分頁：顯示目前筆數、總筆數、頁碼。

**查詢資料**

- 使用 `GetKolListQuery` 類型的查詢取得頁面資料。
- 現有 Query 欄位不足以完整支援 Figma，後續實作需補齊：
  - KOL 狀態。
  - 收款資料狀態。
  - 社群驗證狀態。
  - 建立日期起訖。
  - 可匯出時使用同一組篩選條件。
- DTO 需包含列表列資料與頁首統計摘要；統計摘要不跟隨搜尋與篩選條件，只套用目前資料權限，作為全站 KOL 營運總覽。
- 主要資料來源：`KolProfiles`、`Users`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、`Tasks`、`Submissions`、`Disputes`、`KolEarnings`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 執行篩選 | 以 Query String 帶入篩選條件，重新查詢列表。 |
| 清除搜尋 | 清空所有篩選條件，回到第一頁。 |
| 匯出 KOL 清單 | 依目前篩選條件匯出；欄位需與列表核心欄位一致，是否包含個資需待確認。 |
| 查看待審 KOL | 導向 `ADM-015 審核新進 KOL頁`。 |
| 詳細資料 | 導向 `ADM-006 KOL詳細檔案頁`。 |
| 任務 | 導向該 KOL 的任務清單或任務紀錄頁；目標頁面待 Figma 補齊。 |
| 異議 | 導向該 KOL 的未結案異議列表或異議處理頁；目標頁面待 Figma 補齊。 |

**權限**

- 建議權限碼：`Admin.Kol.View`、`Admin.Kol.Export`、`Admin.Kol.Review`。
- 匯出屬個資風險操作，需寫入操作紀錄，至少包含操作者、篩選條件、匯出時間與筆數。

**狀態與資料規則**

- KOL 狀態以 `KolProfiles.VerificationStatus` 為主；若需區分啟用中與停權中，需與 `Users.Status` 一併判斷。
- 收款資料狀態以 `KolBankAccounts` 是否存在與其狀態判斷。
- 社群驗證狀態以 `KolSocialAccounts.VerificationStatus` 判斷；若多平台狀態不同，列表顯示規則待確認。
- 異議數顯示未結案異議數；異議處理完才能結案，已結案異議不計入列表警示數。

**稽核**

- 一般查詢不寫操作紀錄。
- 匯出、停權/解除停權、代理登入、審核結果等高風險操作必須寫入操作紀錄。

### 14.7 ADM-006 KOL 詳細檔案頁

**頁面目的**

管理者查看單一 KOL 的個人資料、定位、社群頻道、收款資料、任務與異議紀錄、收益摘要與操作紀錄，並可執行停權/解除停權與代理登入。

**主要區塊**

- 頁首摘要：KOL 名稱、狀態、KOL 類型、主要平台、粉絲總數、收款資料狀態、任務數、總收過稿數、未結案異議數、放棄任務次數。
- 功能按鈕：代理登入、返回 KOL 管理。
- 基本資料：KOL 名稱、聯絡窗口、手機、Email、LINE 綁定狀態、建立日期。
- KOL 定位：KOL 類型、自我介紹、可合作條件。
- 社群頻道：平台、帳號名稱、粉絲數、資料來源、最後更新。
- 社群審核提醒：例如 TikTok 頻道是否與實際資料維持相符。
- 收款資料：收款身份、收款戶名、銀行、銀行代號、帳號、收款資料狀態、最近更新。
- 隱私與安全提示：收款資料屬敏感資料，不顯示完整敏感資訊。
- KOL 管理：管理操作原因、停權、解除停權。
- 任務與異議紀錄：任務狀態、案件名稱、合作條件、備註、操作。
- 收益摘要：待月結、待匯款、已匯款、導購分潤累計。
- 收益來源明細：來源項目、金額、狀態、日期。
- 操作紀錄：管理者操作、審核、狀態異動、社群提醒等紀錄。

**查詢資料**

- 使用 `GetKolDetailQuery` 類型的查詢取得頁面資料。
- 現有 `KolDetailDto` 已涵蓋基本資料、狀態、社群帳號、收款資料、近期任務、收益摘要、活動紀錄；後續需補齊：
  - 放棄任務次數。
  - 總收過稿數或通過驗收數。
  - 未結案異議數。
  - 社群審核提醒。
  - 任務與異議紀錄的備註與可操作項目。
  - 收益來源明細。
- 主要資料來源：`KolProfiles`、`Users`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、`Tasks`、`Cases`、`Submissions`、`Disputes`、`KolEarnings`、`ActivityLogs`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 返回 KOL 管理 | 回到 `ADM-005` 並盡量保留原查詢條件。 |
| 代理登入 | 僅保留規格，不在現階段實作。 |
| 停權 | 必填管理操作原因，更新 KOL 狀態並寫入操作紀錄。 |
| 解除停權 | 必填管理操作原因，恢復 KOL 可用狀態並寫入操作紀錄。 |
| 查看案件 | 導向案件詳情或案件監控詳情頁。 |
| 查看異議 | 導向異議處理頁。 |

**權限**

- 建議權限碼：`Admin.Kol.View`、`Admin.Kol.Suspend`、`Admin.Kol.Impersonate`。
- 收款資料屬敏感資料，後台僅可顯示遮罩後資訊；不得顯示完整帳號。
- 停權、解除停權、代理登入必須寫入操作紀錄。

**狀態規則**

- 停權原因必填，寫入 `KolProfiles.SuspensionNote` 或後續專用操作紀錄欄位。
- 停權時同步更新 `KolProfiles.VerificationStatus = Suspended` 與 `Users.Status = Suspended`，禁止 KOL 登入與操作任務。
- 解除停權時先預設回到 `KolProfiles.VerificationStatus = Approved` 與 `Users.Status = Active`；後續若新增停權前狀態欄位或可由操作紀錄穩定推導，再還原停權前狀態。
- 收款帳號在 ADM-006 永遠遮罩，不提供完整帳號顯示；可顯示銀行名稱、戶名、遮罩帳號與收款資料狀態。
- LINE 綁定狀態應區分公開 LINE ID 與 LINE Login/Messaging API 綁定，不可混用欄位。

**稽核**

- 停權/解除停權需記錄操作者、KolId、原因、異動前後狀態、時間。
- 代理登入規格需保留 `AdminId`、`TargetUserId`、`KolId`、開始時間、結束時間、原因、IP、UserAgent、代理期間操作稽核。
- 社群資料若由 API 同步，需記錄資料來源與最後同步時間；人工輸入也需能在操作紀錄追蹤。

### 14.8 ADM-015 審核新進 KOL 頁

**頁面目的**

管理者集中查看待審核、重送審核、退回待補的 KOL，透過篩選快速找到需要處理的申請，並進入審核詳情頁完成審核判斷。

**主要區塊**

- 統計摘要：待審核、重送審核、已退回待補、今日新增、超過 3 日未審。
- 功能按鈕：匯出待審清單、重新整理。
- 搜尋與篩選：
  - 關鍵字：KOL 名稱、Email、手機、社群帳號。
  - 狀態篩選：待審核、重送審核、已退回待補、已通過。
  - KOL 類型。
  - 主要平台。
  - 資料完整度。
  - 社群資料。
  - 送審日期。
- 待審列表：KOL/類型、主要平台、總粉絲數、資料完整度、審核狀態、送審時間、操作。
- 分頁：顯示目前筆數、總筆數、頁碼。

**查詢資料**

- 使用 `GetKolReviewListQuery` 類型的查詢取得頁面資料。
- 現有 Query 欄位不足以完整支援 Figma，後續實作需補齊：
  - 資料完整度篩選。
  - 社群資料篩選。
  - 送審日期。
  - 重送審核、已退回待補等審核狀態細分。
  - 匯出時使用同一組篩選條件。
- DTO 需包含列表資料與頁首統計摘要；統計摘要不跟隨搜尋與篩選條件，只套用目前資料權限，作為待審 KOL 營運總覽。
- 主要資料來源：`KolProfiles`、`Users`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、`ActivityLogs`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 執行篩選 | 以 Query String 帶入篩選條件，重新查詢列表。 |
| 清除篩選 | 清空所有篩選條件，回到第一頁。 |
| 匯出待審清單 | 依目前篩選條件匯出；欄位是否包含手機、Email、社群帳號需待確認。 |
| 重新整理 | 保留目前篩選條件，重新查詢列表。 |
| 進入審核 | 導向 `ADM-016 KOL 審核詳情頁`。 |

**權限**

- 建議權限碼：`Admin.Kol.Review.View`、`Admin.Kol.Review.Export`。
- 匯出屬個資風險操作，需寫入操作紀錄。

**狀態與資料規則**

- 待審核、重送審核、已退回待補都屬審核流程狀態；目前 `VerificationStatus` 無法完整表達重送/待補語意，實作前需決定是否新增欄位或用現有狀態加時間/紀錄推導。
- 資料完整度應以 KOL 個人資料、社群頻道、收款資料、必要聯絡資訊計算；精確公式待確認。
- 超過 3 日未審以送審時間起算 3 個工作日，排除假日。

**稽核**

- 一般查詢不寫操作紀錄。
- 匯出、審核通過、退回修改必須寫入操作紀錄。

### 14.9 ADM-016 KOL 審核詳情頁

**頁面目的**

管理者檢視 KOL 送審資料，確認個人資料、定位、社群頻道、收款資料是否完整且可信，並送出審核通過或退回修改。

**主要區塊**

- 頁首摘要：KOL 名稱、啟用狀態、審核狀態、KOL 類型、主要平台、粉絲總數、收款資料狀態、任務數、總收過稿數、未結案異議數。
- 基本資料：KOL 名稱、聯絡窗口、手機、Email、LINE 綁定狀態、建立日期。
- KOL 定位：KOL 類型、自我介紹、可合作條件。
- 社群頻道：平台、帳號名稱、粉絲數、資料來源、最後更新。
- 社群審核提醒：提醒管理者確認社群頻道與實際帳號是否相符。
- 收款資料：收款身份、收款戶名、銀行、銀行代號、銀行帳號、收款資料狀態、最近更新。
- 隱私與安全提示：收款資料僅供審核，不顯示完整敏感資訊。
- KOL 審核：
  - 審核狀態、目前狀態、送審時間。
  - 退回原因或審核備註。
  - 資料審核意見。
  - 審核通過、退回修改。

**查詢資料**

- 使用 `GetKolDetailQuery` 類型的查詢取得審核詳情資料；若後續 ADM-006 與 ADM-016 DTO 差異擴大，可拆成 `GetKolReviewDetailQuery`。
- 現有 `KolDetailDto` 已可作為基礎，但後續需補齊：
  - 審核狀態細分。
  - 送審時間。
  - 退回原因或退回待補提示。
  - 社群審核提醒。
  - 收款資料遮罩規則。
- 主要資料來源：`KolProfiles`、`Users`、`KolCategories`、`KolSocialAccounts`、`KolBankAccounts`、`ActivityLogs`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 返回 KOL 管理 | 回到 `ADM-005` 或 `ADM-015`；來源頁需透過 returnUrl 或查詢條件保留。 |
| 審核通過 | 使用 `ApproveKolCommand`，將 KOL 審核狀態改為通過並記錄審核者與時間。 |
| 退回修改 | 使用 `RejectKolCommand` 或後續專用退回 Command，退回原因/審核意見必填。 |

**權限**

- 建議權限碼：`Admin.Kol.Review.View`、`Admin.Kol.Review.Approve`、`Admin.Kol.Review.Reject`。
- 收款資料屬敏感資料，審核頁也只能顯示遮罩後資訊；不得顯示完整帳號。

**狀態規則**

- 審核通過後，KOL 應進入可使用狀態；預設更新 `KolProfiles.VerificationStatus = Approved`。
- 退回修改不等同永久拒絕；Figma 顯示「退回原因/審核意見」與「退回修改」，因此需保留 KOL 補件後重送審核的可能。
- 目前 `RejectKolCommand` 將狀態轉為 `VerificationStatus.Rejected`；此處 `Rejected` 定義為「退回修改/已退回待補」，不是永久拒絕，KOL 補件後可重送審核。
- 審核流程採共通語意但不共用 enum：KOL 資料審核使用 `VerificationStatus`，任務成果驗收使用 `SubmissionStatus` 與 `TaskStatus`；不得把 KOL 的 `VerificationStatus.Rejected` 與成果驗收的 `SubmissionStatus.Rejected` 混用。
- 審核意見在退回修改時必填；審核通過時選填。
- 社群資料由 API 同步時，資料來源與最後更新時間需顯示；人工輸入資料不可偽裝為 API 同步。

**稽核**

- 審核通過、退回修改需記錄操作者、KolId、審核前後狀態、審核意見、時間。
- 退回修改需保留最新退回原因，並建議保留歷次退回紀錄供後續追蹤。
- 若審核期間觸發通知，通知發送結果也需能追蹤。

### 14.10 ADM-007 案件監控頁

**頁面目的**

管理者以案件為單位監控平台所有案件，查看案件狀態、招募/成立進度、KOL 任務交付、待驗收、逾期、異議與導購分潤同步狀況，並快速進入案件詳情或業者資料。

**主要區塊**

- 案件狀態統計：全部案件、草稿、招募中、招募滿額、執行中、已結案、已取消。
- 風險提醒卡：
  - 待驗收任務：所有成果已提交，提醒業者於期限內完成驗收。
  - 已逾期任務：包含 KOL 交付逾期與業者驗收逾期，列表需可區分逾期類型。
  - 爭議處理中任務：業者與 KOL 之間存在驗收異議，等待平台介入。
  - 導購同步異常案件：API 同步資料發生異常，導致導購分潤數據無法更新。
  - 四張風險卡都視為快速篩選條件；點擊後留在 ADM-007，帶入 `RiskType` 查詢列表。
- 搜尋與篩選：
  - 關鍵字：案件名稱或業者。
  - 風險類型：`PendingReview`、`Overdue`、`DisputeInProgress`、`AffiliateSyncError`。
  - 導購分潤。
  - 案件狀態：全部、草稿、招募中、執行中、已結案等。
  - 合作條件：現金酬勞、體驗項目、導購分潤。
  - 建立日期範圍。
- 案件列表：案件名稱/業者、案件狀態、合作條件、招募/成立、任務狀態摘要、警示項目、導購狀態、建立日期、操作。
- 分頁：顯示目前筆數、總筆數、頁碼。

**查詢資料**

- 後續需新增 `GetCaseMonitorListQuery` 類型的查詢，或在案件監控模組建立等價 Query。
- DTO 需包含頁首統計、風險提醒統計、目前套用的風險篩選、列表資料與分頁資料。
- Query 需支援 `RiskType` 快速篩選，四張風險卡分別對應 `PendingReview`、`Overdue`、`DisputeInProgress`、`AffiliateSyncError`。
- 主要資料來源：`Cases`、`Merchants`、`CaseApplications`、`Tasks`、`Submissions`、`Disputes`、`KolEarnings`、`MerchantWalletTransactions`、`ActivityLogs`。
- 導購同步異常目前 schema 尚未完整支援；後續需補 `ReferralOrders`、導購同步紀錄或外部 webhook/API 事件紀錄後才能完整實作。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 套用篩選 | 以 Query String 帶入篩選條件，重新查詢列表。 |
| 點擊風險卡 | 留在 ADM-007，帶入對應 `RiskType` 作為快速篩選並重新查詢列表。 |
| 重設篩選 | 清空所有篩選條件，回到第一頁。 |
| 查看詳情 | 導向 `ADM-008 案件詳情與進度頁`。 |
| 查看業者 | 導向 `ADM-003 業者詳細檔案頁`。 |
| 查看異議 | 套用 `RiskType = DisputeInProgress` 快速篩選；若從列表單筆進入，目標頁面依 `ADM-009-DRAWER` 補齊。 |
| 即刻處理 | 套用 `RiskType = Overdue` 快速篩選。 |
| 診斷原因 | 套用 `RiskType = AffiliateSyncError` 快速篩選。 |

**權限**

- 建議權限碼：`Admin.CaseMonitor.View`、`Admin.CaseMonitor.Resolve`、`Admin.Dispute.View`。
- 管理者可查看所有案件，但只應透過明確操作進行人工介入；一般列表查詢不應改變案件或任務狀態。

**狀態與資料規則**

- 案件狀態以 `Cases.Status` 為主，招募滿額可由 `WantedKolCount`、`ApprovedAssignmentCount`、`RecruitmentStatus` 推導。
- 任務狀態摘要需由 `Tasks.Status` 聚合，例如執行中、已交付、驗收通過、驗收逾期。
- 待驗收任務以 `Submissions.Status = Submitted` 且 `ReviewDeadlineAt` 未逾期為主。
- 已逾期任務同時納入 KOL 交付逾期與業者驗收逾期；列表 DTO 需提供逾期類型，讓營運知道應催 KOL 或催業者。
- 爭議處理中任務以 `Disputes.Status` 未結案為主。
- 導購狀態需區分正常、未啟用、異常；若案件未啟用導購分潤，不應顯示為異常。

**稽核**

- 一般查詢不寫操作紀錄。
- 管理者進行人工介入、處理逾期、處理爭議、診斷或重跑導購同步時，必須寫入操作紀錄。

### 14.11 ADM-008 案件詳情與進度頁

**頁面目的**

管理者查看單一案件的完整進度，包含案件基本資料、時程、合作條件、招募與任務統計、KOL 任務清單、導購與分潤統計、附件合約與操作紀錄。

**主要區塊**

- 頁首摘要：案件名稱、案件狀態、業者、合作條件標籤、招募進度、已成立任務、待驗收任務、已逾期任務、爭議處理中、導購狀態。
- 功能按鈕：返回案件監控、查看業者、查看異議任務。
- 頁籤：案件摘要、合作條件、任務統計、KOL 任務清單、交付驗收、導購狀態、附件合約、操作紀錄；頁籤僅作為同頁區塊定位跳轉，不代表後端分頁或分段載入。
- 案件基本資料：案件編號、預定招募人數、曝光平台、案件負責人。
- 時程資訊：案件執行期間、KOL 招募截止、成果交付截止、進度條與剩餘天數。
- 合作條件：現金酬勞、體驗項目、導購分潤。
- 招募與任務統計：報名人數、未入選、已成立任務、執行中、已交付待驗、驗收通過。
- KOL 任務清單：KOL 名稱、KOL 類型、主要平台、任務狀態、成果提交、驗收結果、異議狀態、最近更新、操作。
- 導購與訂單統計：交易總件數、訂單總金額、平均客單價、預估總分潤。
- 附件與合約：附件名稱、大小、上傳日期、下載。
- 操作紀錄：任務狀態變更、驗收結果、成果提交、導購同步等事件。

**查詢資料**

- 後續需新增 `GetCaseMonitorDetailQuery` 類型的查詢，或在案件監控模組建立等價 Query。
- DTO 需包含頁首摘要、各區塊資料、任務清單、導購統計、附件合約、操作紀錄；後端以同一個案件詳情查詢回傳完整資料。
- 主要資料來源：`Cases`、`Merchants`、`CasePlatforms`、`CaseCategories`、`CaseBarterItems`、`CaseBudgetSnapshots`、`CaseApplications`、`Tasks`、`Submissions`、`SubmissionItems`、`Disputes`、`KolProfiles`、`KolEarnings`、`Files`、`CaseAttachments`、`ActivityLogs`。
- 導購統計需依後續 `ReferralOrders` 或等價資料來源補齊；目前 `KolEarnings` 僅能支援收益面，不能完整代表訂單交易面。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 返回案件監控 | 回到 `ADM-007` 並盡量保留原查詢條件。 |
| 查看業者 | 導向 `ADM-003 業者詳細檔案頁`。 |
| 查看異議任務 | 導向異議處理頁或抽屜；目標頁面依 `ADM-009-DRAWER` 補齊。 |
| 匯出表格 | 匯出目前 KOL 任務清單；是否包含個資欄位待確認。 |
| 批次操作 | 對 KOL 任務清單做批次處理；可用動作、條件與稽核要求待確認。 |
| 詳情 | 查看單一 KOL 任務詳情、提交成果或驗收紀錄；目標頁面待 Figma 補齊。 |
| 處理爭議 | 對爭議中的任務進入異議處理流程。 |
| 下載附件 | 下載案件附件或合約，需依檔案權限檢查。 |

**權限**

- 建議權限碼：`Admin.CaseMonitor.View`、`Admin.CaseMonitor.Export`、`Admin.CaseMonitor.Resolve`、`Admin.File.Download`。
- 附件合約可能含合約或個資，下載需寫入操作紀錄。
- 批次操作屬高風險操作，必須先確認可操作狀態與操作範圍，並寫入操作紀錄。

**狀態與資料規則**

- 案件詳情頁應以案件為根節點，所有任務、提交、異議、導購資料都必須限定在同一 `CaseId`。
- 任務狀態、驗收狀態、異議狀態需分開顯示，不可用單一狀態覆蓋所有流程。
- 已交付待驗以 `Submissions.Status = Submitted` 為主；驗收通過以 `Submissions.Status = Approved` 或 `Tasks.Status = Completed` 判斷。
- 爭議處理中以未結案 `Disputes` 判斷。
- 導購狀態若顯示「同步正常」，需有最後同步時間或同步事件作為依據；目前資料表尚待補齊。
- 附件合約需使用 `Files` 與 `CaseAttachments`，不可只存 URL 字串。

**稽核**

- 查看詳情不寫操作紀錄。
- 匯出、批次操作、下載附件、處理爭議、重跑導購同步必須寫入操作紀錄。
- 操作紀錄至少包含操作者、CaseId、TaskId 或 FileId、動作、前後狀態、時間。

### 14.12 ADM-009-DRAWER 異議處理

**頁面目的**

管理者集中查看 KOL 任務驗收、成果、逾期、報酬與導購分潤相關異議，並在右側抽屜內完成平台判定、處理紀錄與結果確認。

**主要區塊**

- 異議摘要：全部異議、待處理、處理中、待補件、已結案、今日新增。
- 搜尋與篩選：
  - 關鍵字：搜尋編號、名稱等。
  - 異議狀態。
  - 異議類型。
- 異議列表：異議編號、異議狀態、異議類型、案件與業者、KOL 名稱、任務狀態、建立時間、操作。
- 右側 Drawer：
  - 異議資訊：異議編號、異議類型、異議狀態。
  - 雙方聯絡方式：業者聯絡資訊、KOL 聯絡資訊、LINE 綁定狀態。
  - 內容對照：案件與任務摘要、原始交付要求、KOL 交付內容、業者主張理由、KOL 異議內容。
  - 處理紀錄：建立異議、補件、平台處理、雙方回覆等時間軸。
  - 平台處理操作：處理結果、平台處理意見、儲存處理紀錄、確認處理結果。

**查詢資料**

- 後續需新增 `GetDisputeListQuery` 與 `GetDisputeDetailQuery` 類型的查詢，或在 Dispute 模組建立等價 Query。
- 列表 DTO 需包含異議編號、狀態、類型、案件/業者、KOL、任務狀態、建立時間。
- Drawer DTO 需包含異議主檔、雙方聯絡資訊、任務/提交內容、雙方說明、處理紀錄、附件。
- 主要資料來源：`Disputes`、`DisputeMessages`、`DisputeAttachments`、`Cases`、`Merchants`、`KolProfiles`、`Users`、`Tasks`、`Submissions`、`SubmissionItems`、`Files`、`ActivityLogs`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 處理爭議 | 開啟右側 Drawer 並載入異議詳情。 |
| 儲存處理紀錄 | 新增一筆平台處理紀錄，不結案，可維持 `UnderReview` 或目前狀態。 |
| 確認處理結果 | 依處理結果更新 `Disputes.Status`，必要時同步更新 `Submissions`、`Tasks`、收益或錢包狀態。 |
| 關閉 | 關閉 Drawer，不寫入資料。 |

**權限**

- 建議權限碼：`Admin.Dispute.View`、`Admin.Dispute.Resolve`。
- 只有具備處理權限的管理者可儲存處理紀錄或確認處理結果。
- Drawer 會顯示雙方聯絡資料，需視為個資畫面；不應在操作紀錄寫入完整手機或 Email。

**狀態與資料規則**

- 異議列表狀態需對應 `DisputeStatus`：
  - `Open` 可顯示為待處理。
  - `UnderReview` 可顯示為處理中。
  - `ResolvedForMerchant`、`ResolvedForKol`、`ResolvedCompromise` 可顯示為已結案。
  - `Cancelled` 可顯示為已取消。
- `退回補件` 是業者端驗收 KOL 交付成果時的操作選項；管理者在 ADM-009 看到的任務狀態為 `待補件`。
- `待補件` 不是新的 `DisputeStatus`，應對應 `Submissions.Status = RevisionRequested` 與 `Tasks.Status = RevisionRequested` 或等價補件狀態。
- ADM-009 統一處理異議類型，包含驗收爭議、成果內容爭議、逾期爭議、報酬爭議與導購分潤爭議。
- 平台處理結果第一版固定為：
  - `維持待補件`：平台認定業者退回補件合理，`Disputes.Status = ResolvedForMerchant`，任務/提交維持或更新為待補件狀態，通知 KOL 重新提交，不直接異動示意帳務。
  - `改判驗收通過`：平台認定 KOL 成果有效，`Disputes.Status = ResolvedForKol`，提交可往通過方向更新，後續收益依既有驗收通過規則建立示意應付紀錄，實際匯款由管理者/財務線下處理。
  - `雙方協議結案`：`Disputes.Status = ResolvedCompromise`，需保存協議內容；若涉及金額調整，必須建立示意帳務/錢包交易流水或人工調整紀錄，實際收付款由管理者/財務線下處理。
  - `取消異議`：`Disputes.Status = Cancelled`，不自動改任務、提交或收益狀態。
- 平台金額、錢包、鎖定額度與 KOL 收益只作為後台營運與財務線下收付款依據；系統不直接執行實際收款或匯款。
- 若確認處理結果會影響示意帳務狀態、鎖定/釋放額度、KOL 應付收益、人工調整紀錄或任務完成/未完成，必須在同一 transaction 中完成。

**稽核**

- 儲存處理紀錄、確認處理結果必須寫入操作紀錄。
- 操作紀錄至少包含操作者、DisputeId、CaseId、TaskId、處理結果、處理意見、前後狀態、時間。
- ADM-009 的平台處理意見只作管理員內部紀錄，不顯示給 KOL 或業者。
- `DisputeMessages` 可用於保存雙方留言與平台處理紀錄；平台內部處理意見需以可見性欄位或內部紀錄類型標記，避免被前台讀取。
- 若處理結果觸發通知，通知發送結果也需能追蹤。

### 14.13 ADM-011 帳務監控頁

**頁面目的**

管理者以案件結算為單位監控平台資金收入、KOL 支出與平台毛利，並可展開查看單一案件的 KOL 任務報酬、導購佣金、稅金/手續費與淨付金額。

**主要區塊**

- 平台財務摘要：
  - 平台總收入：總資金收入，主要來自業者儲值、案件預算、平台服務費、折扣或調整後收入。
  - 平台總支出：經濟支出，主要為 KOL 淨付金額與可能的退款/調整。
  - 平台毛利：平台營收扣除 KOL 支出、退款、折扣與必要調整後的毛利。
- 功能按鈕：匯出報表。
- 搜尋與篩選：
  - 關鍵字：案件編號、案件名稱、業者名稱。
  - 監控時間範圍。
  - 狀態：依案件狀態篩選，包含草稿、招募中、招募截止、執行中、已完成、已結案、已取消。
- 帳務列表：
  - 結算日期。
  - 業者名稱。
  - 案件名稱與案件編號。
  - 收入總金額（業者）。
  - 支出資金（KOL）。
  - 狀態：顯示案件狀態；若有異議或導購同步異常，另以警示標籤呈現。
  - 操作：案件詳情。
- 展開任務明細：
  - KOL 姓名。
  - 任務報酬（Mission）。
  - 導購佣金（Affiliate）。
  - 稅金/手續費。
  - 淨付金額。
  - 狀態。
- 分頁：顯示目前筆數、總筆數、頁碼。

**查詢資料**

- 後續需新增 `GetFinanceMonitorListQuery` 類型的查詢，或在 Finance/Wallet/Payout 模組建立等價 Query。
- DTO 需包含平台財務摘要、案件帳務列、可展開的 KOL 任務明細與分頁資料。
- 主要資料來源：
  - `Cases`、`Merchants`：案件與業者資訊。
  - `MerchantWalletTransactions`：業者儲值、案件預算凍結、釋放、結算、人工調整。
  - `KolEarnings`、`KolWallets`：KOL 任務報酬、導購分潤、收益狀態。
  - `PayoutRequests`: KOL 提領與撥款狀態。
  - `Tasks`、`Submissions`：任務完成與驗收狀態。
  - `ActivityLogs`：帳務資料異動與人工調整紀錄。
- 導購佣金若需從訂單交易計算，需依後續 `ReferralOrders` 或等價資料來源補齊；`KolEarnings` 僅代表 KOL 收益結果，不足以完整還原訂單收入。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 套用篩選 | 以 Query String 帶入篩選條件，重新查詢列表。 |
| 匯出報表 | 依目前篩選條件匯出帳務報表。 |
| 展開/收合案件 | 查詢或顯示該案件的 KOL 任務帳務明細。 |
| 案件詳情 | 導向 `ADM-008 案件詳情與進度頁`。 |

**權限**

- 建議權限碼：`Admin.Finance.View`、`Admin.Finance.Export`、`Admin.CaseMonitor.View`。
- 帳務監控含金額與個人收益資料，需限制為具財務權限的管理者。
- 匯出報表屬高風險操作，需寫入操作紀錄，至少包含操作者、篩選條件、匯出時間與筆數。

**狀態與資料規則**

- 平台總收入、平台總支出、平台毛利都必須明確定義統計期間；若未選期間，預設範圍需待確認。
- 收入總金額（業者）應能拆解費用明細，例如內容預算、KOL 分潤、折扣金、平台服務費、人工調整；畫面中的費用明細需由交易流水或結算快照提供，不得只顯示手算字串。
- 收入費用明細中的折扣金使用獨立折扣金錢包與交易流水，不與現金錢包混算。
- 支出資金（KOL）應以 KOL 淨付金額彙總，需扣除稅金/手續費或平台費用後計算。
- 任務明細中的淨付金額應由任務報酬、導購佣金、稅金/手續費計算得出，並需與 `KolEarnings.NetAmount` 或結算明細一致。
- 稅金/手續費由後台參數設定頁維護相關費率；案件結算時需保存費率快照，ADM-011 顯示的是結算結果與快照，不在此頁手動輸入費率。
- 目前沒有獨立帳務狀態；ADM-011 的狀態欄以案件狀態為主，需遵循 PM 案件狀態表：草稿、招募中、招募截止、執行中、已完成、已結案、已取消。
- `已結案` 代表所有帳務與流程完成；在案件進入 `已結案` 前，若仍有未完成帳務、異議或導購同步異常，應以警示標籤或阻擋結案規則呈現，不另新增帳務狀態。
- 所有金額欄位沿用 `DECIMAL(12,2)` 與 NTD 顯示規則；匯出時也需保留小數精度。

**稽核**

- 一般查詢不寫操作紀錄。
- 匯出報表需寫入操作紀錄。
- 若後續此頁加入人工調整、重算結算或重跑分潤，必須在同一 transaction 中更新交易流水與彙總資料，並寫入操作紀錄。

### 14.14 ADM-012 系統參數設定頁

**頁面目的**

- 提供後台管理者維護全平台商務參數，供案件建立、預算估算、導購分潤、KOL 提領與關帳規則使用。
- 此頁修改的是平台預設值；依 Figma 提示，變更後只影響未來新建立的案件與使用者行為，已成立的訂單、案件任務與既有結算不得被回溯改寫。

**主要區塊**

- 系統全局設定提醒：說明參數為平台全局預設值，修改後僅影響未來資料。
- 核心商務參數：
  - 案件開案費（NT$）。
  - KOL 服務費率（%）。
  - 平台抽成比例（%）。
  - KOL 最低分潤比例（%）。
  - 案件自動執行門檻（%）。
  - 最低提領金額（NT$）。
  - 稅金/手續費相關費率。
  - 提領方式。
  - 撥款日（每月幾號）。
  - 關帳日設定。
- 名詞與業務定義：
  - 現金酬勞：業者直接支付給 KOL 的固定現金報酬，不包含後續導購抽成。
  - 體驗項目：業者提供的商品或服務，作為實物報酬，平台不對體驗項目的價值進行抽成，僅收取開案費。
  - 導購分潤：透過專屬連結產生的訂單金額，依據設定比例分配給 KOL 與平台。
- 近期異動紀錄：顯示最近的參數變更，欄位包含異動時間、操作人員、異動項目、原設定值、新設定值、備註。

**查詢資料**

- `GetSystemSettingsQuery`
  - 回傳核心商務參數、顯示用名詞定義，以及是否可編輯。
  - 建議直接讀取 `SystemSettings`，依 `Group` 或固定 key 分組後回傳。
- `GetRecentSystemSettingLogsQuery`
  - 回傳最近 N 筆異動紀錄。
  - 對應 `SystemSettingLogs`，需包含操作者、異動 key、原值、新值、備註與時間。
- `GetSystemSettingLogsQuery`
  - 供「查看完整紀錄」使用。
  - 對應獨立完整異動紀錄頁，後端需支援分頁查詢。

**參數 Key 建議**

- `case_opening_fee_amount`：案件開案費，`ValueType = decimal`。
- `kol_service_fee_rate`：KOL 服務費率，`ValueType = percent`。
- `affiliate_platform_commission_rate`：平台抽成比例，`ValueType = percent`；導購訂單成立後平台固定保留此比例。
- `affiliate_kol_min_commission_rate`：KOL 最低分潤比例，`ValueType = percent`；與平台抽成比例相加後作為業者開案時可輸入的最低總佣金比例。
- `case_auto_execution_threshold_rate`：案件自動執行門檻，`ValueType = percent`。
- `kol_min_payout_amount`：最低提領金額，`ValueType = decimal`。
- `kol_tax_rate`：KOL 稅金扣除率，`ValueType = percent`。
- `kol_payout_fee_rate`：KOL 提領或付款手續費率，`ValueType = percent`。
- `kol_payout_fixed_fee_amount`：KOL 提領或付款固定手續費，`ValueType = decimal`。
- `kol_payout_mode`：提領方式，截圖目前為「全額提領」。
- `kol_payout_days`：撥款日，截圖格式為 `10, 25`。
- `kol_payout_closing_day_offset`：關帳日設定，例如撥款日前 5 日可存為 `-5` 或獨立 enum。

**預估凍結金額公式**

- 案件開案費與 KOL 服務費率用於案件發布預估凍結金額。
- 公式：`EstimatedFrozenAmount = (RewardAmountPerKol * WantedKolCount * KolServiceFeeRate) + CaseOpeningFeeAmount`。
- `RewardAmountPerKol` 為單人/組 KOL 費用，`WantedKolCount` 為預計組數，`KolServiceFeeRate` 與 `CaseOpeningFeeAmount` 來自本頁系統參數。
- 計算明細需保存到 `CaseBudgetSnapshots.FeeItems` 與 `CaseBudgetSnapshots.SettingsSnapshot`，避免後續參數調整影響已發布案件。

**導購佣金比例公式**

- 業者開案時輸入的 `Cases.CommissionRate` 為導購總佣金比例。
- 最低可輸入總佣金比例：`MinimumCommissionRate = AffiliatePlatformCommissionRate + AffiliateKolMinCommissionRate`。
- 平台取得比例固定為 `AffiliatePlatformCommissionRate`。
- KOL 取得比例為 `KolCommissionRate = CommissionRate - AffiliatePlatformCommissionRate`。
- 因此若業者輸入剛好等於最低比例，KOL 取得 `AffiliateKolMinCommissionRate`；若業者輸入高於最低比例，超出最低比例的部分全數歸 KOL。

**提交動作**

- `UpdateSystemSettingsCommand`
  - 對應「儲存設定」。
  - 必須以交易方式一次更新所有送出的參數。
  - 每個實際變更的 key 都要新增一筆 `SystemSettingLogs`，未變更的 key 不需新增紀錄。
  - 若部分欄位驗證失敗，整批不得寫入。
- `ResetSystemSettingsToDefaultCommand`
  - 對應「還原預設」。
  - 預設值來源使用 `SystemSettings.DefaultValue`；意思是每個參數除了目前值 `Value`，也保存一份可還原的預設值。
  - 還原也屬於設定異動，必須寫入 `SystemSettingLogs`。

**權限**

- 讀取頁面需 `Admin.SystemSettings.View`。
- 儲存設定與還原預設需 `Admin.SystemSettings.Manage`。
- 查看完整異動紀錄沿用 `Admin.SystemSettings.View` 權限。

**資料表**

- 主要沿用 `SystemSettings`：
  - 最低欄位需求：`Key`、`Value`、`DefaultValue`、`ValueType`、`Group`、`Description`、`UpdatedByUserId`、`UpdatedAt`。
  - 案件開案費與 KOL 服務費率已定案用於案件發布預估凍結金額。
  - 平台抽成比例、KOL 最低分潤比例已定義為導購佣金拆分規則；schema 可先以 `0` 初始化，正式營運值由後台手動設定。
- 異動紀錄沿用 `SystemSettingLogs`：
  - 最低欄位需求：`SettingKey`、`OldValue`、`NewValue`、`ChangedByUserId`、`ChangedAt`、`Note`。
- 案件建立與結算流程需在自己的快照欄位保存當時設定：
  - 案件預算建議使用既有 `CaseBudgetSnapshots.SettingsSnapshot`。
  - 導購分潤與提領若會跨期結算，也需要對應的結算快照，避免後續參數調整影響已成立資料。

**狀態與驗證規則**

- Figma 有紅色 `* 必填` 的欄位皆不得為空。
- 金額欄位需為大於或等於 0 的數值；最低提領金額建議大於 0。
- 百分比欄位需為 0 到 100 之間的數值。
- `affiliate_platform_commission_rate + affiliate_kol_min_commission_rate` 需小於或等於 100，且作為 MER-003 業者輸入導購佣金比例的最低值。
- 案件自動執行規則：招募截止時，錄取人數大於或等於預計招募人數的一半，案件可自動進入執行中。
- 撥款日可接受多個日期，格式為逗號分隔的 1 到 31 整數，需去重並排序後儲存。
- 撥款日遇假日順延至下一個工作日。
- 關帳日設定需能換算成每次撥款日前的截止日期，`撥款日前 5 日` 以工作日計算。
- 所有與日期相關的排程建議統一使用台北時區作為業務日基準。

**稽核紀錄**

- 每次儲存或還原預設，都要記錄到 `SystemSettingLogs`。
- 同時新增 `ActivityLogs`，讓後台操作紀錄能查到「誰在何時修改系統參數」。
- 若參數變更會影響後續排程，例如提領、關帳、分潤計算，排程執行時需記錄使用的設定版本或設定快照摘要。

**缺頁或缺功能提醒**

- 「查看完整紀錄」採獨立頁面呈現，支援分頁、查詢與稽核閱讀。
- 目前設計未明確呈現「設定版本」；若財務與分潤需要可追溯，建議後端在快照中保存設定 key/value 與生效時間。
- 參數儲存後立即對未來資料生效；既有案件、訂單、任務與結算依各自快照，不回溯改寫。

### 14.15 ADM-013 後台帳號管理頁

**頁面目的**

- 管理後台內部使用者、系統層級角色、帳號狀態、邀請狀態與異動紀錄。
- 此頁處理的是 `AccountType = Admin` 的帳號，不包含業者成員與 KOL 帳號。

**主要區塊**

- 統計摘要：
  - 全部帳號。
  - 啟用中。
  - 邀請中。
  - 停用中。
  - 邀請過期。
- 篩選列：
  - 關鍵字：姓名、Email、部門。
  - 帳號狀態。
  - 部門。
  - 最後登入。
  - 角色快速篩選：系統管理者、主管檢視、業務、平台營運、財務會計。
- 後台帳號列表：
  - 姓名、Email、部門、角色、帳號狀態、最後登入、建立時間、操作。
  - 操作包含編輯、停用、啟用、重新寄送邀請。
- 最近異動日誌：
  - 時間、操作者、異動對象、異動類型、異動內容、備註。
- 新增/編輯後台帳號 modal：
  - 基本資料、角色設定、帳號狀態、備註。

**查詢資料**

- `GetAdminAccountListQuery`
  - 篩選條件：`Keyword`、`Status`、`Department`、`LastLoginRange`、`RoleIds`、`Page`、`PageSize`。
  - 回傳列表、統計摘要與分頁資訊。
  - 主要來源為 `Users`、`UserRoles`、`Roles`，限定 `Users.AccountType = Admin` 與 `Roles.Scope = System`。
- `GetAdminAccountEditQuery`
  - 編輯時取得單一後台帳號、目前角色、部門、職稱、聯絡電話、狀態、備註。
- `GetAdminRolesQuery`
  - 回傳可指派的系統角色，限定 `Roles.Scope = System` 且 `IsActive = true`。
- `GetRecentAdminAccountActivityLogsQuery`
  - 回傳最近帳號管理異動紀錄，可先沿用 `ActivityLogs`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 新增後台帳號 | 建立 Admin 帳號邀請，寫入使用者基本資料、角色與邀請資訊。 |
| 儲存帳號 | 更新 Admin 帳號基本資料、角色、狀態與備註。 |
| 儲存並寄送邀請 | 儲存資料後產生邀請 token，寄送邀請信。 |
| 停用 | 將帳號改為停用，禁止登入後台。 |
| 啟用 | 將停用帳號恢復為可登入狀態。 |
| 重新寄送邀請 | 針對尚未完成啟用或邀請過期的帳號重新產生邀請 token 並寄信。 |

**Command 建議**

- `CreateAdminAccountInvitationCommand`
  - 對應「新增後台帳號」與「儲存並寄送邀請」。
  - 需建立 `Users`（`AccountType = Admin`，未設定密碼時 `PasswordHash = null`）、`UserRoles`，並建立邀請 token。
- `UpdateAdminAccountCommand`
  - 更新姓名、Email、部門、職稱、電話、角色、狀態與備註。
  - 更新角色時需重建或差異更新 `UserRoles`。
- `SuspendAdminAccountCommand`
  - 將 `Users.Status` 更新為 `Suspended`。
- `ActivateAdminAccountCommand`
  - 將 `Users.Status` 更新為 `Active`。
- `ResendAdminAccountInvitationCommand`
  - 重新產生邀請 token、更新過期時間並寄送邀請信。

**欄位與資料表**

- 既有資料表可直接使用：
  - `Users`：姓名、Email、帳號類型、登入狀態、最後登入、建立時間。
  - `Roles`：系統角色，需 `Scope = System`。
  - `UserRoles`：後台帳號與系統角色的關聯。
  - `ActivityLogs`：帳號管理異動紀錄。
- 新增 `AdminProfiles` 保存後台帳號延伸資料，不直接加在 `Users`：
  - 部門。
  - 職稱。
  - 聯絡電話。
  - 帳號備註。
- 現有 schema 只有 `MerchantInvitations`，沒有後台帳號邀請表。ADM-013 需新增通用 `UserInvitations`，支援邀請中、邀請過期與重新寄送邀請：
  - `UserId` 或 `Email`。
  - `InvitedByUserId`。
  - `TokenHash`。
  - `Status`：Pending、Accepted、Expired、Cancelled。
  - `ExpiresAt`、`AcceptedAt`、`CreatedAt`。

**狀態規則**

- 登入帳號狀態沿用 `UserStatus`：
  - `Active`：啟用中，可登入後台。
  - `Suspended`：停用中，不可登入後台。
  - `Deleted`：已刪除，不顯示於一般列表，除非後續有稽核查詢需求。
- 邀請狀態不應直接塞進 `Users.Status`：
  - 邀請中與邀請過期應由 invitation 表推導。
  - 邀請尚未完成時，若已先建立 `Users`，該帳號不得可登入，直到完成設定密碼或接受邀請。
- 後台帳號邀請 token 有效期限為 7 天；重新寄送邀請後，舊 token 必須失效。
- 角色快速篩選由 `Roles.Scope = System` 且 `IsActive = true` 的角色動態產生。
- 後台帳號允許同時擁有多個系統角色，沿用 `UserRoles`。
- 停用帳號需立即讓既有後台 session 失效。
- 啟用最後一位系統管理者保護規則；不允許停用、刪除或移除最後一個具備最高管理權限的帳號。
- Email 必須全系統唯一，沿用 `Users.Email` unique constraint。

**權限**

- 查看列表需 `Admin.Account.View`。
- 新增、編輯與重新寄送邀請需 `Admin.Account.Manage`。
- 停用與啟用需 `Admin.Account.ChangeStatus`。
- 修改系統管理者角色建議另設 `Admin.Account.ManageSystemAdmin`，避免一般管理者任意提升權限。

**稽核紀錄**

- 下列操作都必須寫入 `ActivityLogs`：
  - 新增後台帳號。
  - 寄送或重新寄送邀請。
  - 編輯基本資料。
  - 新增/移除角色。
  - 停用/啟用帳號。
- 異動內容需包含異動前後角色、狀態與目標帳號；敏感資料如邀請 token 不可寫入 log。

**缺頁或缺功能提醒**

- Figma 目前只看到帳號管理與新增/編輯 modal，尚未看到邀請信接受頁、設定密碼頁、邀請過期頁；若後台邀請流程要完整上線，前台或後台需補這些入口。

### 14.16 ADM-014 後台角色管理頁

**頁面目的**

- 管理後台角色模板與單一角色的權限矩陣，供 `ADM-013 後台帳號管理頁` 指派給 Admin 帳號。
- 此頁只管理系統層級角色，對應 `Roles.Scope = System`；業者組織角色不在此頁處理。

**主要區塊**

- 統計摘要：
  - 角色模板數。
  - 啟用角色。
  - 停用角色。
  - 使用中帳號。
  - 高風險權限角色。
- 篩選列：
  - 關鍵字：角色名稱或角色說明。
  - 角色狀態。
  - 是否系統保留。
  - 是否高風險角色。
- 角色列表：
  - 角色名稱、角色說明、使用帳號數、角色狀態、系統保留、高風險權限、最近異動、操作。
- 角色異動紀錄：
  - 時間、操作者、異動類型、異動對象、異動內容、備註。
- 角色權限設定頁：
  - 基本資料設定：角色名稱、角色說明、啟用狀態。
  - 權限矩陣清單：依功能項目列出「檢視、編輯、審核管理」等權限欄位。
  - 高風險權限安全性警示。
  - 最近變更紀錄。

**查詢資料**

- `GetAdminRoleListQuery`
  - 篩選條件：`Keyword`、`Status`、`IsSystemReserved`、`HasHighRiskPermission`、`Page`、`PageSize`。
  - 回傳統計摘要、角色列表與分頁資訊。
  - 主要來源為 `Roles`、`UserRoles`、`RolePermissions`、`Permissions`，限定 `Roles.Scope = System`。
- `GetAdminRolePermissionEditQuery`
  - 回傳角色基本資料、可用權限矩陣、目前已勾選權限、高風險權限提示與最近變更紀錄。
- `GetAdminPermissionMatrixQuery`
  - 回傳所有後台權限項目，供新增角色時初始化矩陣。
  - 權限項目建議由 `Permissions.Code` 依功能群組整理，不建議硬寫在 View。
- `GetAdminRoleChangeLogsQuery`
  - 回傳角色異動紀錄，可先沿用 `ActivityLogs`。

**提交動作**

| 動作 | 後端語意 |
| ---- | -------- |
| 新增角色 | 建立 `Roles.Scope = System` 的角色，並寫入權限矩陣。 |
| 編輯 | 進入角色權限設定頁。 |
| 儲存權限設定 | 更新角色基本資料、啟用狀態與 `RolePermissions`。 |
| 返回角色管理 | 不寫入資料，回到 ADM-014 列表。 |
| 查看完整日誌 | 查詢角色異動紀錄完整分頁。 |

**Command 建議**

- `CreateAdminRoleCommand`
  - 建立系統角色與初始權限。
  - `Name + Scope` 必須唯一，沿用 `Roles` unique constraint。
- `UpdateAdminRoleCommand`
  - 更新角色名稱、說明、啟用狀態與權限集合。
  - 權限更新需以 transaction 先比對差異，再更新 `RolePermissions`。
- `DisableAdminRoleCommand`
  - 將 `Roles.IsActive = false`。
  - 可停用仍有帳號使用中的角色；下一次權限檢查不再載入該角色權限，但不得停用最後一個具備最高管理能力的角色。
- `EnableAdminRoleCommand`
  - 將 `Roles.IsActive = true`。

**欄位與資料表**

- 既有資料表可使用：
  - `Roles`：角色名稱、作用範圍、啟用狀態、建立與更新時間。
  - `Permissions`：權限代碼與說明。
  - `RolePermissions`：角色與權限的對應。
  - `UserRoles`：計算使用帳號數。
  - `ActivityLogs`：角色異動紀錄。
- 現有 `Roles` 尚未包含下列畫面欄位，若要完整符合 Figma 需補欄位或建立角色設定表：
  - 角色說明：補在 `Roles.Description`。
  - 系統保留：補在 `Roles.IsSystemReserved`。
- 高風險權限由 `Permissions.RiskLevel` 標記，不由角色手動標記。
- 第一版權限項目由 seed data 固定維護，不提供後台新增/編輯 `Permissions` 的頁面。
- 權限矩陣建議以 `Permissions.Code` 的命名規則分組，例如：
  - `Admin.Merchant.View`、`Admin.Merchant.Edit`。
  - `Admin.Kol.View`、`Admin.Kol.Review`、`Admin.Kol.Suspend`。
  - `Admin.CaseMonitor.View`、`Admin.Dispute.Manage`。
  - `Admin.Finance.View`、`Admin.Finance.Export`、`Admin.Payout.Approve`。
  - `Admin.Account.View`、`Admin.Account.Manage`。
  - `Admin.Role.View`、`Admin.Role.Manage`。

**權限矩陣規則**

- 「檢視」代表可讀取列表與詳情。
- 「編輯」代表可修改資料、建立資料或執行一般管理動作。
- 「審核管理」代表可執行高影響流程，例如 KOL 審核、停權、異議處理、付款審核、權限調整。
- 若勾選「全選所有權限」，後端仍需依目前登入者可授予的權限範圍過濾，不可讓低權限管理者提升出自己沒有的權限。
- 系統保留角色不可刪除；可編輯名稱與說明，但不可移除核心管理權限。
- 高風險權限角色需依 `Permissions.RiskLevel` 與角色權限集合推導，包含但不限於：
  - 完整帳務資料檢視。
  - 停權 KOL 操作。
  - 完整身分證字號檢視。
  - 完整身分證字號修改。
  - 角色與權限管理。
  - 後台帳號管理。
  - 付款/撥款審核。

**狀態與保護規則**

- 停用角色後，不應再允許新帳號指派該角色。
- 已持有停用角色的帳號在下一次權限檢查時立即不再載入該角色權限。
- 不允許刪除或停用最後一個具備 `Admin.Role.Manage` 與 `Admin.Account.ManageSystemAdmin` 的角色。
- 不允許操作者透過編輯角色移除自己最後的權限管理能力，除非另有更高權限管理者仍存在。
- 低權限管理者不得授予自己沒有的權限；儲存權限矩陣時必須以目前操作者可授予的權限範圍過濾。
- 權限矩陣儲存時需記錄異動前後差異，避免只留下「已更新」這類無法稽核的摘要。

**權限**

- 查看列表需 `Admin.Role.View`。
- 新增、編輯、啟用、停用角色需 `Admin.Role.Manage`。
- 修改高風險權限需 `Admin.Role.ManageHighRisk` 或等價二階權限。
- 查看完整異動紀錄需 `Admin.Role.ViewLogs` 或沿用 view 權限；完整紀錄採獨立頁面規格，但目前 Figma 尚未提供畫面。

**稽核紀錄**

- 下列操作都必須寫入 `ActivityLogs`：
  - 新增角色。
  - 修改角色名稱、說明、啟用狀態。
  - 新增/移除權限。
  - 啟用/停用角色。
  - 修改高風險權限。
- 異動內容需包含 role id、role name、異動前權限集合、異動後權限集合、操作者與備註。

**缺頁或缺功能提醒**

- Figma 目前只看到「查看完整日誌」入口，尚未看到完整角色異動紀錄頁；後端先保留獨立頁面查詢規格。
- 第一版權限項目固定由 seed data 維護；目前不需要後台新增/編輯 Permission 的頁面。


