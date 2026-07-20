namespace Domain.Enums;

/// <summary>
/// 業者折扣金交易類型。
/// 1=Grant 發放
/// 2=Use 折抵使用
/// 3=Refund 退回
/// 4=Revoke 扣回
/// 5=Expire 到期
/// 6=ManualAdjustment 人工調整
/// </summary>
public enum MerchantCreditTransactionType : short
{
    Grant = 1,
    Use = 2,
    Refund = 3,
    Revoke = 4,
    Expire = 5,
    ManualAdjustment = 6
}
