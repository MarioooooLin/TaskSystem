using Application.Abstractions.Security;
using Domain.Enums;
using System.Security.Claims;

namespace Admin.Extensions;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
        => _accessor = accessor;

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public bool IsAuthenticated
        => User?.Identity?.IsAuthenticated == true;

    public long UserId
        => long.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("未登入或 UserId Claim 不存在。");

    public AccountType AccountType
        => Enum.TryParse<AccountType>(User?.FindFirstValue("account_type"), out var t)
            ? t
            : throw new InvalidOperationException("AccountType Claim 不存在。");

    public IReadOnlyList<string> Permissions
        => User?.FindAll("permission").Select(c => c.Value).ToList()
           ?? [];

    public bool HasPermission(string permissionCode)
        => Permissions.Contains(permissionCode);
}
