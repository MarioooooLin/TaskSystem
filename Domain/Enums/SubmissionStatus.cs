namespace Domain.Enums;

public enum SubmissionStatus : short
{
    Submitted = 1,          // 已提交，待業者驗收
    RevisionRequested = 2,  // 業者退回修改
    Approved = 3,           // 驗收通過
    Rejected = 4,           // 驗收不通過
    Overdue = 5,            // 超過驗收期限（14 天未處理）
    Disputed = 6            // 管理者介入爭議中
}
