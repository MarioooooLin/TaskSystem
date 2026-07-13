using Application.Abstractions.Persistence;
using Application.AdminAccounts.DTOs;
using Common.Pagination;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

/// <summary>後台帳號管理 Repository。</summary>
public interface IAdminAccountRepository
{
    /// <summary>分頁查詢後台帳號列表。</summary>
    Task<(IReadOnlyList<AdminAccountListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        UserStatus? status,
        string? department,
        long? roleId,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得後台帳號 KPI 摘要。</summary>
    Task<AdminAccountSummaryDto> GetSummaryAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>取得單一後台帳號編輯資料。</summary>
    Task<AdminAccountEditDto?> GetByIdAsync(long userId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得可指派的系統角色選項。</summary>
    Task<IReadOnlyList<AdminRoleOptionDto>> GetActiveSystemRolesAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>以 Email 查詢後台帳號。</summary>
    Task<User?> GetUserByEmailAsync(string email, IDbSession session, CancellationToken ct = default);

    /// <summary>建立後台帳號使用者。</summary>
    Task<long> InsertUserAsync(User user, IDbSession session, CancellationToken ct = default);

    /// <summary>更新後台帳號使用者。</summary>
    Task UpdateUserAsync(User user, IDbSession session, CancellationToken ct = default);

    /// <summary>建立或更新後台帳號延伸資料（UPSERT）。</summary>
    Task UpsertProfileAsync(long userId, string? department, string? jobTitle, string? phone, string? note, IDbSession session, CancellationToken ct = default);

    /// <summary>重建後台帳號的系統角色（先刪除後插入）。</summary>
    Task ReplaceRolesAsync(long userId, IEnumerable<long> roleIds, IDbSession session, CancellationToken ct = default);

    /// <summary>建立邀請記錄。</summary>
    Task<long> InsertInvitationAsync(UserInvitation invitation, IDbSession session, CancellationToken ct = default);

    /// <summary>將指定使用者的邀請標記為 Cancelled。</summary>
    Task CancelInvitationsByUserAsync(long userId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得最進 N 筆帳號管理異動紀錄。</summary>
    Task<IReadOnlyList<AdminAccountLogItemDto>> GetRecentLogsAsync(int count, IDbSession session, CancellationToken ct = default);

    /// <summary>取得目前仍有效的最後一筆管理員帳號 UserId（用於最後一位系統管理者保護）。</summary>
    Task<long?> GetLastActiveSystemAdminUserIdAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>指定帳號是否擁有最高管理權限角色（含所有系統權限視為 Super Admin）。</summary>
    Task<bool> HasSystemAdminRoleAsync(long userId, IDbSession session, CancellationToken ct = default);
}
