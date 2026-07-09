namespace Application.Account;

/// <summary>
/// Admin 登入指令。
/// 回傳 LoginResult 供 Controller 寫入 Claims。
/// </summary>
public sealed record LoginCommand(string Email, string Password);

/// <summary>
/// 登入成功後回傳的使用者資料，Controller 用來建立 Claims。
/// </summary>
public sealed record LoginResult(
    long UserId,
    string Name,
    string Email,
    IReadOnlyList<string> PermissionCodes
);
