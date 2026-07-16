namespace Application.Account;

/// <summary>
/// 設定密碼頁面初始資料。
/// </summary>
public sealed record SetPasswordInitDto(
    string Name,
    string Email);
