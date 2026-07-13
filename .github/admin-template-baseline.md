# Admin Template Baseline

> 建立日期：2026-07-13
> 用途：記錄目前 `Admin/Template/` 內既有 HTML template，作為後續設計師補頁後的差異比對基準。

## 比對方式

- 後續設計師補上 template 後，先比對 `Admin/Template/` 內新增、刪除、改名的 `.html` 檔案。
- 若只是更新既有檔案內容，需另行檢查該檔對應的 `.cshtml` 是否需要同步調整。
- `Admin/Template/` 目前混有 Admin 後台頁、共用片段，以及業者/KOL 端 template；比對新增頁面時需先依下方分類判斷。

## 目前 Admin 後台頁面 Template

| Template | 畫面/用途 | 目前對應或推定 View |
| --- | --- | --- |
| `index.html` | 營運總覽 | `Dashboard/Index` |
| `business-management.html` | 業者管理 | `MerchantManagement/Index` |
| `business-detail.html` | 業者詳細檔案 | `MerchantManagement/Detail` |
| `business-edit.html` | 編輯業者資料 | `MerchantManagement/Update` |
| `kol-management.html` | KOL 管理 | `KolManagement/Index` |
| `kol-detail.html` | KOL 詳細檔案 | `KolManagement/Detail` |
| `kol-new.html` | 審核新進 KOL | `KolManagement/ReviewList` |
| `kol-review.html` | KOL 審核詳情 | `KolManagement/ReviewDetail` |
| `cases-monitor.html` | 案件監控 | `CaseMonitor/Index` |
| `case-detail.html` | 案件詳情與進度 | `CaseMonitor/Detail` |
| `wallet.html` | 帳務/錢包管理 | `Finance/Index` |
| `transaction-history.html` | 交易紀錄明細 | `Finance/Transactions` |
| `settings.html` | 設定中心 / 系統參數設定 | `SystemSetting/Index` |
| `permission.html` | 使用者與權限管理 | `AdminAccount/Index` 或權限相關列表 |
| `add-user.html` | 新增成員 | `AdminAccount/Create` |
| `permission-management.html` | 角色權限管理 | `RolePermission/Index` / `RolePermission/Detail` |
| `login.html` | 登入 | `Account/Login` |
| `forgot-pw.html` | 忘記密碼 | `Account/ForgotPassword` |

## 共用片段 / Layout Template

| Template | 用途 |
| --- | --- |
| `header.html` | Header 片段 |
| `footer.html` | Footer 片段 |
| `sidebar.html` | Sidebar 片段 |
| `sub-header.html` | Sub header 片段 |

## 目前放在 Admin/Template 但不列為 Admin 後台頁

| Template | 畫面/用途 |
| --- | --- |
| `accepting.html` | 成果驗收 |
| `add-cases.html` | 新增案件 |
| `cases.html` | 案件管理 |
| `cases-detail.html` | 案件詳情 |
| `company-info.html` | 企業資料維護 |
| `info.html` | 未確認用途，需視內容判斷 |
| `notification-settings.html` | 通知偏好設定 |
| `publish.html` | 發佈案件確認 |
| `referral.html` | 導購成效總覽 |
| `referral-detail.html` | 導購明細頁 |
| `register.html` | 申請註冊 |

## 目前未看到明確 Template 的 Admin 缺頁 / 待確認頁

- `Dispute/Detail`：`MEMORY.md` 2026-07-10 記錄列表「處理爭議」目前導向 `/Dispute/Detail/{id}`，但 `Admin/Template/` 無對應 detail template，暫緩等待 PM/設計確認。
- `ADM-011` 帳務總覽展開明細：目前 `Admin/Template/` 無帳務總覽專屬切版，內嵌子表格為功能占位，視覺樣式待設計師確認。
- `SystemSetting` 完整異動紀錄頁：`admin-pages.md` 記錄「查看完整紀錄」採獨立頁面，但目前未見明確 template。
- `AdminAccount` 邀請接受頁、設定密碼頁、邀請過期頁：`admin-pages.md` 記錄 Figma 尚未看到完整邀請流程頁。
- `RolePermission` 完整角色異動紀錄頁：`admin-pages.md` 記錄 Figma 目前只看到「查看完整日誌」入口，尚未看到完整日誌頁。

## 2026-07-13 設計師補頁後確認

### 新增 HTML template

| Template | 畫面/用途 | 建議對應 View / 模組 | 備註 |
| --- | --- | --- | --- |
| `account.html` | 綜合財務監控面板 | `Finance/Index` 或財務監控新版 | 對應 `account.css`。可能取代原本以 `wallet.html` 占位的 ADM-011。 |
| `manager.html` | 後台帳號管理 | `AdminAccount/Index` | 對應 `manager.css`。補齊後台帳號管理列表/操作畫面。 |
| `objection.html` | 異議處理 | `Dispute/Detail` 或 `Dispute/Index` + detail drawer | 對應 `objection.css`。補齊先前缺少的爭議/異議處理 template。 |
| `parameter.html` | 參數設定 | `SystemSetting/Index` / `SystemSetting/Parameters` | 對應 `parameter.css`。比原 `settings.html` 更貼近 ADM-012 參數設定頁。 |
| `permission-setting.html` | 角色權限設定 | `RolePermission/Detail` | 對應 `permission-setting.css`。補齊角色權限矩陣設定頁。 |

### 既有 HTML template 有更新

| Template | 觀察 |
| --- | --- |
| `case-detail.html` | 案件詳情與進度 template 有內容更新。 |
| `header.html` | 後台 header / 導覽片段有內容更新。 |
| `kol-detail.html` | KOL 詳細檔案 template 有內容更新。 |
| `kol-review.html` | KOL 審核詳情 template 有內容更新。 |
| `permission.html` | 標題已變為「後台角色管理」，用途更接近 `RolePermission/Index`，不再像原本「使用者與權限管理」。 |

### 新增或更新的資源目錄

- `Admin/Template/css/` 目前包含新頁對應 CSS：`account.css`、`manager.css`、`objection.css`、`parameter.css`、`permission-setting.css`。
- `Admin/Template/images/` 與 `Admin/Template/js/` 也出現在本次 template 資源差異中，需在整合 cshtml 時確認實際引用路徑。