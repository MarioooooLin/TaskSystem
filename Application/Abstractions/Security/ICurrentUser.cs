using Domain.Enums;

namespace Application.Abstractions.Security;

/// <summary>
/// 目前登入者資訊，由各 MVC 專案實作並注入。
/// Use Case 透過此介面取得操作者身份，不直接讀取 HttpContext。
/// </summary>
public interface ICurrentUser
{
    /// <summary>UserId（對應 Users.Id）。</summary>
    long UserId { get; }

    /// <summary>帳號類型（Kol / Merchant / Admin）。</summary>
    AccountType AccountType { get; }

    /// <summary>是否已登入。</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 擁有的 Permission Code 清單（登入時載入 Claims）。
    /// 例如：["Merchant.Case.Create", "Merchant.Application.Accept"]
    /// </summary>
    IReadOnlyList<string> Permissions { get; }

    /// <summary>
    /// 是否擁有指定的 Permission。
    /// </summary>
    bool HasPermission(string permissionCode);
}
