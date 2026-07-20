namespace Domain.Enums;

/// <summary>
/// 業者現金錢包交易類型。
/// 1=OfflineDeposit 線下儲值
/// 2=TaskBudgetFreeze 任務預算鎖定
/// 3=TaskBudgetRelease 任務預算釋放
/// 4=TaskBudgetSettle 任務預算結算撥款
/// 5=DisputeHold 爭議保留
/// 6=ManualAdjustment 人工調整
/// </summary>
public enum MerchantWalletTransactionType : short
{
    OfflineDeposit = 1,
    TaskBudgetFreeze = 2,
    TaskBudgetRelease = 3,
    TaskBudgetSettle = 4,
    DisputeHold = 5,
    ManualAdjustment = 6
}
