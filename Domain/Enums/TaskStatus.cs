namespace Domain.Enums;

public enum TaskStatus : short
{
    PendingMatch = 1,       // 待媒合：任務已建立，尚未綁定 KOL
    PendingExecution = 2,   // 待執行：已綁定 KOL，尚未開始
    InProgress = 3,         // 執行中：KOL 執行任務中
    UnderReview = 4,        // 驗收中：已提交成果，等待業者驗收
    RevisionRequested = 5,  // 修改中：業者退回，等待 KOL 重新提交
    Completed = 6,          // 已完成：驗收通過
    Incomplete = 7,         // 未完成：驗收失敗或逾期
    Cancelled = 8           // 已取消：案件取消
}
