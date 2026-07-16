namespace Application.Account;

/// <summary>
/// 個人帳號頁面資料。
/// </summary>
public sealed record ProfileDto(
    string Name,
    string Email,
    string RolesDisplay);
