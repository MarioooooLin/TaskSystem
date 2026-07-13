using Domain.Enums;

namespace Application.AdminAccounts.DTOs;

/// <summary>編輯後台帳號時所需的單筆資料 DTO。</summary>
public sealed record AdminAccountEditDto
{
    public long UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Department { get; init; }
    public string? JobTitle { get; init; }
    public string? Phone { get; init; }
    public string? Note { get; init; }
    public UserStatus Status { get; init; }
    public IReadOnlyList<long> RoleIds { get; init; } = [];
}
