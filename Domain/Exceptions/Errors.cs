using Common.Errors;

namespace Domain.Exceptions;

/// <summary>
/// 全域預定義錯誤碼。
/// Application Use Case 回傳 Result.Failure 時使用此處定義的 Error，
/// 不在各處散落硬編碼字串。
/// </summary>
public static class Errors
{
    public static class Case
    {
        public static readonly Error NotFound =
            Error.NotFound("Case.NotFound", "案件不存在。");

        public static readonly Error AlreadyPublished =
            Error.Conflict("Case.AlreadyPublished", "案件已發布，無法重複發布。");

        public static readonly Error CannotCancel =
            Error.Problem("Case.CannotCancel", "案件進入執行中後不允許取消。");

        public static readonly Error CannotModify =
            Error.Problem("Case.CannotModify", "案件進入執行中後不允許修改。");

        public static readonly Error DeadlineInvalid =
            Error.Validation("Case.DeadlineInvalid", "報名截止日必須早於成果截止日。");

        public static readonly Error InsufficientBalance =
            Error.Problem("Case.InsufficientBalance", "業者餘額不足，無法發布案件。");
    }

    public static class Application
    {
        public static readonly Error NotFound =
            Error.NotFound("Application.NotFound", "報名紀錄不存在。");

        public static readonly Error AlreadyApplied =
            Error.Conflict("Application.AlreadyApplied", "KOL 已報名此案件。");

        public static readonly Error CannotAccept =
            Error.Problem("Application.CannotAccept", "此報名狀態不允許接受。");

        public static readonly Error CannotReject =
            Error.Problem("Application.CannotReject", "此報名狀態不允許拒絕。");

        public static readonly Error NoAvailableTask =
            Error.Problem("Application.NoAvailableTask", "目前沒有可綁定的 PendingMatch Task。");
    }

    public static class Submission
    {
        public static readonly Error NotFound =
            Error.NotFound("Submission.NotFound", "成果提交紀錄不存在。");

        public static readonly Error CannotReview =
            Error.Problem("Submission.CannotReview", "此成果狀態不允許進行驗收。");

        public static readonly Error DeadlineExceeded =
            Error.Problem("Submission.DeadlineExceeded", "已超過提交截止日期。");
    }

    public static class Task
    {
        public static readonly Error NotFound =
            Error.NotFound("Task.NotFound", "任務不存在。");

        public static readonly Error InvalidStatusTransition =
            Error.Problem("Task.InvalidStatusTransition", "任務狀態不允許此操作。");
    }

    public static class Merchant
    {
        public static readonly Error NotFound =
            Error.NotFound("Merchant.NotFound", "業者不存在。");

        public static readonly Error NotApproved =
            Error.Forbidden("Merchant.NotApproved", "業者尚未通過審核。");

        public static readonly Error AlreadySuspended =
            Error.Conflict("Merchant.AlreadySuspended", "業者已處於停用狀態。");

        public static readonly Error NotSuspended =
            Error.Conflict("Merchant.NotSuspended", "業者目前並非停用狀態。");
    }

    public static class Kol
    {
        public static readonly Error NotFound =
            Error.NotFound("Kol.NotFound", "KOL 不存在。");

        public static readonly Error AlreadyApproved =
            Error.Conflict("Kol.AlreadyApproved", "KOL 已通過審核。");

        public static readonly Error AlreadySuspended =
            Error.Conflict("Kol.AlreadySuspended", "KOL 已處於停權狀態。");

        public static readonly Error NotSuspended =
            Error.Conflict("Kol.NotSuspended", "KOL 目前並非停權狀態。");

        public static readonly Error CannotApprove =
            Error.Problem("Kol.CannotApprove", "此 KOL 狀態不允許審核通過（需為 Pending 或 Rejected）。");
    }

    public static class Member
    {
        public static readonly Error NotFound =
            Error.NotFound("Member.NotFound", "成員不存在。");

        public static readonly Error NotActive =
            Error.Forbidden("Member.NotActive", "成員帳號已停用或移除。");

        public static readonly Error NotBelongToMerchant =
            Error.Forbidden("Member.NotBelongToMerchant", "此成員不屬於指定業者組織。");
    }

    public static class Role
    {
        public static readonly Error NotFound =
            Error.NotFound("Role.NotFound", "角色不存在。");

        public static readonly Error InvalidScope =
            Error.Problem("Role.InvalidScope", "此角色不適用於指派給業者成員。");

        public static readonly Error PermissionDenied =
            Error.Forbidden("Role.PermissionDenied", "您沒有管理角色的權限。");

        public static readonly Error NameRequired =
            Error.Validation("Role.NameRequired", "角色名稱為必填。");

        public static readonly Error DuplicateName =
            Error.Conflict("Role.DuplicateName", "相同作用範圍下已有相同名稱的角色。");

        public static readonly Error PermissionNotFound =
            Error.Validation("Role.PermissionNotFound", "部分權限項目不存在。");
    }

    public static class Wallet
    {
        public static readonly Error NotFound =
            Error.NotFound("Wallet.NotFound", "錢包不存在。");

        public static readonly Error InsufficientBalance =
            Error.Problem("Wallet.InsufficientBalance", "可用餘額不足。");
    }

    public static class Payout
    {
        public static readonly Error NotFound =
            Error.NotFound("Payout.NotFound", "撥款申請不存在。");

        public static readonly Error AlreadyProcessed =
            Error.Conflict("Payout.AlreadyProcessed", "撥款申請已處理。");
    }

    public static class Dispute
    {
        public static readonly Error NotFound =
            Error.NotFound("Dispute.NotFound", "爭議案件不存在。");

        public static readonly Error AlreadyOpen =
            Error.Conflict("Dispute.AlreadyOpen", "此任務已有進行中的爭議。");
    }

    public static class SystemSetting
    {
        public static readonly Error NotFound =
            Error.NotFound("SystemSetting.NotFound", "系統參數不存在。");

        public static readonly Error InvalidValue =
            Error.Validation("SystemSetting.InvalidValue", "參數值格式或範圍不正確。");

        public static readonly Error CommissionRateSumExceeded =
            Error.Validation("SystemSetting.CommissionRateSumExceeded", "平台抽成比例與 KOL 最低分潤比例總和不可超過 100%。");
    }

    public static class User
    {
        public static readonly Error NotFound =
            Error.NotFound("User.NotFound", "使用者不存在。");

        public static readonly Error Forbidden =
            Error.Forbidden("User.Forbidden", "無權限執行此操作。");

        public static readonly Error InvalidCredentials =
            Error.Validation("User.InvalidCredentials", "電子郵件或密碼錯誤。");

        public static readonly Error AccountSuspended =
            Error.Forbidden("User.AccountSuspended", "帳號已停用，請聯繫系統管理員。");

        public static readonly Error NotAdminAccount =
            Error.Forbidden("User.NotAdminAccount", "此帳號無法登入後台。");

        public static readonly Error EmailAlreadyExists =
            Error.Conflict("User.EmailAlreadyExists", "此電子郵件已被使用。");

        public static readonly Error LastSystemAdmin =
            Error.Forbidden("User.LastSystemAdmin", "無法停用或刪除最後一個系統管理者。");
    }

    public static class AdminAccount
    {
        public static readonly Error NotFound =
            Error.NotFound("AdminAccount.NotFound", "後台帳號不存在。");

        public static readonly Error InvalidEmail =
            Error.Validation("AdminAccount.InvalidEmail", "電子郵件格式不正確。");

        public static readonly Error RoleRequired =
            Error.Validation("AdminAccount.RoleRequired", "請至少選擇一個角色。");

        public static readonly Error RoleNotFound =
            Error.NotFound("AdminAccount.RoleNotFound", "指定的系統角色不存在或已停用。");

        public static readonly Error AlreadyActive =
            Error.Conflict("AdminAccount.AlreadyActive", "帳號已為啟用狀態。");

        public static readonly Error AlreadySuspended =
            Error.Conflict("AdminAccount.AlreadySuspended", "帳號已處於停用狀態。");

        public static readonly Error InvitationAlreadyAccepted =
            Error.Conflict("AdminAccount.InvitationAlreadyAccepted", "邀請已被接受。");
    }

    public static class Email
    {
        public static readonly Error SendFailed =
            Error.Problem("Email.SendFailed", "郵件發送失敗，請確認郵件設定或稍後再試。");
    }

    public static class Invitation
    {
        public static readonly Error NotFound =
            Error.NotFound("Invitation.NotFound", "邀請連結無效或已失效。");

        public static readonly Error Expired =
            Error.Problem("Invitation.Expired", "邀請連結已過期，請聯繫管理員重新發送邀請。");

        public static readonly Error AlreadyAccepted =
            Error.Conflict("Invitation.AlreadyAccepted", "此邀請已被使用，請直接登入。");
    }
}
