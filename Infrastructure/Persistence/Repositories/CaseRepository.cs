using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class CaseRepository : ICaseRepository
{
    public async Task<Case?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                Id, MerchantId, CreatedByUserId,
                Title, Description, OfficialUrl, City, Address,
                WantedKolCount, ApplicationDeadline, SubmissionDeadline,
                CashRewardAmount, IsCommissionEnabled, CommissionRate, CookieDays,
                DeliverableDescription, Status, RecruitmentStatus,
                AutoExecutionThresholdRate, AutoExecutionThresholdCount,
                ApplicationCount, ApprovedAssignmentCount,
                PublishedAt, StartedAt, CompletedAt, SettledAt, CancelledAt,
                CreatedAt, UpdatedAt
            FROM Cases
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Case>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<Case?> GetByIdAndMerchantAsync(long id, long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                Id, MerchantId, CreatedByUserId,
                Title, Description, OfficialUrl, City, Address,
                WantedKolCount, ApplicationDeadline, SubmissionDeadline,
                CashRewardAmount, IsCommissionEnabled, CommissionRate, CookieDays,
                DeliverableDescription, Status, RecruitmentStatus,
                AutoExecutionThresholdRate, AutoExecutionThresholdCount,
                ApplicationCount, ApprovedAssignmentCount,
                PublishedAt, StartedAt, CompletedAt, SettledAt, CancelledAt,
                CreatedAt, UpdatedAt
            FROM Cases WITH (UPDLOCK)
            WHERE Id = @Id AND MerchantId = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Case>(
            sql, new { Id = id, MerchantId = merchantId }, session.Transaction);
    }

    public async Task<long> InsertAsync(Case caseEntity, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Cases (
                MerchantId, CreatedByUserId,
                Title, Description, OfficialUrl, City, Address,
                WantedKolCount, ApplicationDeadline, SubmissionDeadline,
                CashRewardAmount, IsCommissionEnabled, CommissionRate, CookieDays,
                DeliverableDescription, Status, RecruitmentStatus,
                AutoExecutionThresholdRate, AutoExecutionThresholdCount,
                ApplicationCount, ApprovedAssignmentCount,
                PublishedAt, StartedAt, CompletedAt, SettledAt, CancelledAt,
                CreatedAt, UpdatedAt
            ) VALUES (
                @MerchantId, @CreatedByUserId,
                @Title, @Description, @OfficialUrl, @City, @Address,
                @WantedKolCount, @ApplicationDeadline, @SubmissionDeadline,
                @CashRewardAmount, @IsCommissionEnabled, @CommissionRate, @CookieDays,
                @DeliverableDescription, @Status, @RecruitmentStatus,
                @AutoExecutionThresholdRate, @AutoExecutionThresholdCount,
                @ApplicationCount, @ApprovedAssignmentCount,
                @PublishedAt, @StartedAt, @CompletedAt, @SettledAt, @CancelledAt,
                GETUTCDATE(), GETUTCDATE()
            );
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql, caseEntity, session.Transaction);
    }

    public async Task UpdateAsync(Case caseEntity, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Cases SET
                Title = @Title,
                Description = @Description,
                OfficialUrl = @OfficialUrl,
                City = @City,
                Address = @Address,
                WantedKolCount = @WantedKolCount,
                ApplicationDeadline = @ApplicationDeadline,
                SubmissionDeadline = @SubmissionDeadline,
                CashRewardAmount = @CashRewardAmount,
                IsCommissionEnabled = @IsCommissionEnabled,
                CommissionRate = @CommissionRate,
                CookieDays = @CookieDays,
                DeliverableDescription = @DeliverableDescription,
                Status = @Status,
                RecruitmentStatus = @RecruitmentStatus,
                AutoExecutionThresholdRate = @AutoExecutionThresholdRate,
                AutoExecutionThresholdCount = @AutoExecutionThresholdCount,
                ApplicationCount = @ApplicationCount,
                ApprovedAssignmentCount = @ApprovedAssignmentCount,
                PublishedAt = @PublishedAt,
                StartedAt = @StartedAt,
                CompletedAt = @CompletedAt,
                SettledAt = @SettledAt,
                CancelledAt = @CancelledAt,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, caseEntity, session.Transaction);
    }

    public async Task<CaseEditData?> GetEditDataAsync(long caseId, long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string caseSql = """
            SELECT
                Id, MerchantId, CreatedByUserId,
                Title, Description, OfficialUrl, City, Address,
                WantedKolCount, ApplicationDeadline, SubmissionDeadline,
                CashRewardAmount, IsCommissionEnabled, CommissionRate, CookieDays,
                DeliverableDescription, Status, RecruitmentStatus,
                AutoExecutionThresholdRate, AutoExecutionThresholdCount,
                ApplicationCount, ApprovedAssignmentCount,
                PublishedAt, StartedAt, CompletedAt, SettledAt, CancelledAt,
                CreatedAt, UpdatedAt
            FROM Cases
            WHERE Id = @CaseId AND MerchantId = @MerchantId
            """;

        var caseEntity = await session.Connection.QueryFirstOrDefaultAsync<Case>(
            caseSql, new { CaseId = caseId, MerchantId = merchantId }, session.Transaction);

        if (caseEntity is null) return null;

        var data = new CaseEditData { Case = caseEntity };

        const string categoriesSql = "SELECT Category FROM CaseCategories WHERE CaseId = @CaseId";
        data.Categories = (await session.Connection.QueryAsync<int>(
            categoriesSql, new { CaseId = caseId }, session.Transaction)).AsList();

        const string languagesSql = "SELECT LanguageCode FROM CaseLanguages WHERE CaseId = @CaseId";
        data.Languages = (await session.Connection.QueryAsync<string>(
            languagesSql, new { CaseId = caseId }, session.Transaction)).AsList();

        const string platformsSql = "SELECT Platform FROM CasePlatforms WHERE CaseId = @CaseId";
        data.Platforms = (await session.Connection.QueryAsync<short>(
            platformsSql, new { CaseId = caseId }, session.Transaction)).AsList();

        const string barterSql = """
            SELECT Id, CaseId, Name, Quantity, Note
            FROM CaseBarterItems
            WHERE CaseId = @CaseId
            """;
        data.BarterItems = (await session.Connection.QueryAsync<CaseBarterItem>(
            barterSql, new { CaseId = caseId }, session.Transaction)).AsList();

        const string reqSql = """
            SELECT Id, CaseId, MinFollowers, Notes
            FROM CaseRequirements
            WHERE CaseId = @CaseId
            """;
        data.Requirements = await session.Connection.QueryFirstOrDefaultAsync<CaseRequirements>(
            reqSql, new { CaseId = caseId }, session.Transaction);

        const string attachmentsSql = """
            SELECT
                ca.Id, ca.CaseId, ca.FileId, ca.Type, ca.CreatedAt,
                f.Id, f.UploadedByUserId, f.FileName, f.FilePath, f.FileSize, f.MimeType, f.CreatedAt
            FROM CaseAttachments ca
            INNER JOIN Files f ON f.Id = ca.FileId
            WHERE ca.CaseId = @CaseId
            ORDER BY ca.CreatedAt DESC
            """;
        data.Attachments = (await session.Connection.QueryAsync<CaseAttachment, FileEntity, CaseAttachment>(
            attachmentsSql,
            (ca, f) =>
            {
                ca.File = f;
                return ca;
            },
            new { CaseId = caseId },
            session.Transaction)).AsList();

        return data;
    }

    public async Task<CaseBudgetSnapshot?> GetLatestBudgetSnapshotAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP 1
                Id, CaseId, RewardAmountPerKol, WantedKolCount, RewardSubtotal,
                FeeItems, EstimatedFrozenAmount, SettingsSnapshot, IdempotencyKey, CreatedAt
            FROM CaseBudgetSnapshots
            WHERE CaseId = @CaseId
            ORDER BY CreatedAt DESC, Id DESC
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseBudgetSnapshot>(
            sql, new { CaseId = caseId }, session.Transaction);
    }

    public async Task<bool> ExistsBudgetSnapshotByIdempotencyKeyAsync(string idempotencyKey, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP 1 1
            FROM CaseBudgetSnapshots
            WHERE IdempotencyKey = @IdempotencyKey
            """;

        return await session.Connection.ExecuteScalarAsync<int?>(
            sql, new { IdempotencyKey = idempotencyKey }, session.Transaction) == 1;
    }

    public async Task SyncSubtablesAsync(
        long caseId,
        IReadOnlyList<int> categories,
        IReadOnlyList<string> languages,
        IReadOnlyList<short> platforms,
        IReadOnlyList<CaseBarterItemInput> barterItems,
        int? minFollowers,
        string? requirementNotes,
        IDbSession session,
        CancellationToken ct = default)
    {
        // Categories
        await session.Connection.ExecuteAsync(
            "DELETE FROM CaseCategories WHERE CaseId = @CaseId",
            new { CaseId = caseId }, session.Transaction);

        if (categories.Count > 0)
        {
            const string categorySql = "INSERT INTO CaseCategories (CaseId, Category) VALUES (@CaseId, @Category)";
            await session.Connection.ExecuteAsync(
                categorySql,
                categories.Select(c => new { CaseId = caseId, Category = c }),
                session.Transaction);
        }

        // Languages
        await session.Connection.ExecuteAsync(
            "DELETE FROM CaseLanguages WHERE CaseId = @CaseId",
            new { CaseId = caseId }, session.Transaction);

        if (languages.Count > 0)
        {
            const string languageSql = "INSERT INTO CaseLanguages (CaseId, LanguageCode) VALUES (@CaseId, @LanguageCode)";
            await session.Connection.ExecuteAsync(
                languageSql,
                languages.Select(l => new { CaseId = caseId, LanguageCode = l }),
                session.Transaction);
        }

        // Platforms
        await session.Connection.ExecuteAsync(
            "DELETE FROM CasePlatforms WHERE CaseId = @CaseId",
            new { CaseId = caseId }, session.Transaction);

        if (platforms.Count > 0)
        {
            const string platformSql = "INSERT INTO CasePlatforms (CaseId, Platform) VALUES (@CaseId, @Platform)";
            await session.Connection.ExecuteAsync(
                platformSql,
                platforms.Select(p => new { CaseId = caseId, Platform = p }),
                session.Transaction);
        }

        // BarterItems
        await session.Connection.ExecuteAsync(
            "DELETE FROM CaseBarterItems WHERE CaseId = @CaseId",
            new { CaseId = caseId }, session.Transaction);

        if (barterItems.Count > 0)
        {
            const string barterSql = """
                INSERT INTO CaseBarterItems (CaseId, Name, Quantity, Note)
                VALUES (@CaseId, @Name, @Quantity, @Note)
                """;
            await session.Connection.ExecuteAsync(
                barterSql,
                barterItems.Select(b => new
                {
                    CaseId = caseId,
                    b.Name,
                    b.Quantity,
                    b.Note
                }),
                session.Transaction);
        }

        // Requirements
        await session.Connection.ExecuteAsync(
            "DELETE FROM CaseRequirements WHERE CaseId = @CaseId",
            new { CaseId = caseId }, session.Transaction);

        if (minFollowers.HasValue || !string.IsNullOrWhiteSpace(requirementNotes))
        {
            const string reqSql = """
                INSERT INTO CaseRequirements (CaseId, MinFollowers, Notes)
                VALUES (@CaseId, @MinFollowers, @Notes)
                """;
            await session.Connection.ExecuteAsync(
                reqSql,
                new { CaseId = caseId, MinFollowers = minFollowers, Notes = requirementNotes },
                session.Transaction);
        }
    }
}
