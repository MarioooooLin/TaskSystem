using Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;
using Domain.Enums;
using System.Security.Claims;

namespace Infrastructure.Authentication;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public bool IsAuthenticated
        => User?.Identity?.IsAuthenticated == true;

    public long UserId
        => long.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("未登入或 UserId Claim 不存在。");

    public AccountType AccountType
        => Enum.TryParse<AccountType>(User?.FindFirstValue(TaskSystemClaimTypes.AccountType), out var t)
            ? t
            : throw new InvalidOperationException("AccountType Claim 不存在。");

    public IReadOnlyList<string> Permissions
        => User?.FindAll(TaskSystemClaimTypes.Permission).Select(c => c.Value).ToList()
           ?? [];

    public bool HasPermission(string permissionCode)
        => Permissions.Contains(permissionCode);
}

