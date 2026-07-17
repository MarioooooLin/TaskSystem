namespace Application.Account;

/// <summary>
/// 業者端登入指令。
/// 回傳 MerchantLoginResult 供 Controller 寫入 Claims。
/// </summary>
public sealed record MerchantLoginCommand(string TaxId, string Email, string Password);

/// <summary>
/// 業者端登入成功後回傳的使用者資料，Controller 用來建立 Claims。
/// </summary>
public sealed record MerchantLoginResult(
    long UserId,
    string Name,
    string Email,
    long MerchantId,
    string CompanyName,
    IReadOnlyList<string> PermissionCodes
);
