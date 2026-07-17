using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Common.Pagination;
using Dapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantRepository : IMerchantRepository
{
    // ── GetByIdAsync ──────────────────────────────────────────────
    public async Task<Merchant?> GetByIdAsync(
        long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, UserId, CompanyName, EnglishName, TaxId, IndustryType,
                   ContactName, Phone, Fax, CompanyEmail, Website, Address,
                   EstablishedDate, VerificationStatus, VerifiedAt,
                   UpdatedByAdminId, CreatedAt, UpdatedAt
            FROM Merchants
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Merchant>(
            sql, new { Id = id }, session.Transaction);
    }

    // ── GetByUserIdAsync ──────────────────────────────────────────
    public async Task<Merchant?> GetByUserIdAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, UserId, CompanyName, EnglishName, TaxId, IndustryType,
                   ContactName, Phone, Fax, CompanyEmail, Website, Address,
                   EstablishedDate, VerificationStatus, VerifiedAt,
                   UpdatedByAdminId, CreatedAt, UpdatedAt
            FROM Merchants
            WHERE UserId = @UserId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Merchant>(
            sql, new { UserId = userId }, session.Transaction);
    }

    // ── GetByTaxIdAsync ───────────────────────────────────────────
    public async Task<Merchant?> GetByTaxIdAsync(
        string taxId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, UserId, CompanyName, EnglishName, TaxId, IndustryType,
                   ContactName, Phone, Fax, CompanyEmail, Website, Address,
                   EstablishedDate, VerificationStatus, VerifiedAt,
                   UpdatedByAdminId, CreatedAt, UpdatedAt
            FROM Merchants
            WHERE TaxId = @TaxId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Merchant>(
            sql, new { TaxId = taxId }, session.Transaction);
    }

    // ── GetListAsync ──────────────────────────────────────────────
    public async Task<(IReadOnlyList<MerchantListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        VerificationStatus? verificationStatus, string? industryType,
        DateTime? dateFrom,
        bool? hasCredit, PageQuery page,
        IDbSession session,
        CancellationToken ct = default)
    {
        var where = BuildWhereClause(keyword, verificationStatus, industryType, dateFrom, hasCredit);

        var countSql = $"""
            SELECT COUNT(*)
            FROM Merchants m
            JOIN Users u ON u.Id = m.UserId
            LEFT JOIN MerchantCreditWallets mcw ON mcw.MerchantId = m.Id
            {where}
            """;

        var dataSql = $"""
            SELECT
                m.Id                   AS MerchantId,
                m.CompanyName,
                m.TaxId,
                m.IndustryType,
                m.ContactName,
                m.Phone,
                u.Email                AS OwnerEmail,
                m.VerificationStatus,
                m.CreatedAt,
                ISNULL(w.AvailableAmount, 0)   AS AvailableAmount,
                ISNULL(mcw.AvailableAmount, 0) AS CreditAmount,
                0                      AS CaseCount
            FROM Merchants m
            JOIN  Users              u ON u.Id = m.UserId
            LEFT JOIN MerchantWallets    w   ON w.MerchantId = m.Id
            LEFT JOIN MerchantCreditWallets mcw ON mcw.MerchantId = m.Id
            {where}
            ORDER BY m.CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = verificationStatus.HasValue ? (short)verificationStatus.Value : (short?)null,
            IndustryType = industryType,
            DateFrom = dateFrom,
            Offset = page.Offset,
            PageSize = page.PageSize,
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        var items = await session.Connection.QueryAsync<MerchantListItemDto>(
            dataSql, param, session.Transaction);

        return (items.AsList(), totalCount);
    }

    // ── GetSummaryAsync ──────────────────────────────────────────
    public async Task<MerchantSummaryDto> GetSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*)                                              AS TotalCount,
                SUM(CASE WHEN VerificationStatus = 2 THEN 1 ELSE 0 END) AS ActiveCount,
                SUM(CASE WHEN VerificationStatus = 4 THEN 1 ELSE 0 END) AS SuspendedCount
            FROM Merchants
            """;

        return await session.Connection.QueryFirstAsync<MerchantSummaryDto>(
            sql, transaction: session.Transaction);
    }

    // ── GetDetailBaseAsync ────────────────────────────────────────
    public async Task<MerchantDetailBaseDto?> GetDetailBaseAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                m.Id                   AS MerchantId,
                m.CompanyName,
                m.EnglishName,
                m.TaxId,
                m.IndustryType,
                m.ContactName,
                m.Phone,
                m.Fax,
                m.CompanyEmail,
                m.Website,
                m.Address,
                m.EstablishedDate,
                u.Email                AS OwnerEmail,
                u.Name                 AS OwnerName,
                m.VerificationStatus,
                m.VerifiedAt,
                adm.Name               AS UpdatedByAdminName,
                m.CreatedAt,
                m.UpdatedAt
            FROM Merchants m
            JOIN  Users u   ON u.Id   = m.UserId
            LEFT JOIN Users adm ON adm.Id = m.UpdatedByAdminId
            WHERE m.Id = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantDetailBaseDto>(
            sql, new { MerchantId = merchantId }, session.Transaction);
    }

    // ── InsertAsync ───────────────────────────────────────────────
    public async Task<long> InsertAsync(
        Merchant merchant, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Merchants
                (UserId, CompanyName, EnglishName, TaxId, IndustryType,
                 ContactName, Phone, Fax, CompanyEmail, Website, Address,
                 EstablishedDate, VerificationStatus)
            VALUES
                (@UserId, @CompanyName, @EnglishName, @TaxId, @IndustryType,
                 @ContactName, @Phone, @Fax, @CompanyEmail, @Website, @Address,
                 @EstablishedDate, @VerificationStatus);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql, merchant, session.Transaction);
    }

    // ── UpdateAsync ───────────────────────────────────────────────
    public async Task UpdateAsync(
        Merchant merchant, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Merchants SET
                CompanyName        = @CompanyName,
                EnglishName        = @EnglishName,
                TaxId              = @TaxId,
                IndustryType       = @IndustryType,
                ContactName        = @ContactName,
                Phone              = @Phone,
                Fax                = @Fax,
                CompanyEmail       = @CompanyEmail,
                Website            = @Website,
                Address            = @Address,
                EstablishedDate    = @EstablishedDate,
                VerificationStatus = @VerificationStatus,
                VerifiedAt         = @VerifiedAt,
                UpdatedByAdminId   = @UpdatedByAdminId,
                UpdatedAt          = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, merchant, session.Transaction);
    }

    // ── 私有：動態 WHERE ──────────────────────────────────────────
    private static string BuildWhereClause(
        string? keyword,
        VerificationStatus? status,
        string? industryType,
        DateTime? dateFrom,
        bool? hasCredit)
    {
        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(keyword))
            conditions.Add("(m.CompanyName LIKE @Keyword OR m.TaxId LIKE @Keyword OR m.ContactName LIKE @Keyword OR u.Email LIKE @Keyword)");

        if (status.HasValue)
            conditions.Add("m.VerificationStatus = @Status");

        if (!string.IsNullOrWhiteSpace(industryType))
            conditions.Add("m.IndustryType = @IndustryType");

        if (dateFrom.HasValue)
            conditions.Add("m.CreatedAt >= @DateFrom");

        if (hasCredit == true)
            conditions.Add("ISNULL(mcw.AvailableAmount, 0) > 0");
        else if (hasCredit == false)
            conditions.Add("ISNULL(mcw.AvailableAmount, 0) = 0");

        return conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : string.Empty;
    }
}
