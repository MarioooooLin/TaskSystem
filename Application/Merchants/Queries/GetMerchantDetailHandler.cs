using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Common.Results;
using Domain.Exceptions;

namespace Application.Merchants.Queries;

public sealed class GetMerchantDetailHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo,
    IMerchantContactRepository contactRepo,
    IMerchantStatsRepository statsRepo,
    IMerchantWalletRepository walletRepo,
    IMerchantMemberRepository memberRepo)
{
    private const int RecentCount = 10;

    public async Task<Result<MerchantDetailDto>> HandleAsync(
        GetMerchantDetailQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 1. 基本資料
        var baseDto = await merchantRepo.GetDetailBaseAsync(query.MerchantId, uow.Session, ct);
        if (baseDto is null)
            return Errors.Merchant.NotFound;

        // 2. 依序查詢子集合，避免同一連線同時開啟多個 DataReader。
        var contacts = await contactRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var stats = await statsRepo.GetStatsByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var cases = await statsRepo.GetRecentCasesAsync(query.MerchantId, RecentCount, uow.Session, ct);
        var wallet = await walletRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var transactions = await walletRepo.GetRecentTransactionsAsync(query.MerchantId, RecentCount, uow.Session, ct);
        var members = await memberRepo.GetMemberListAsync(query.MerchantId, uow.Session, ct);
        var logs = await statsRepo.GetRecentActivityLogsAsync(query.MerchantId, RecentCount, uow.Session, ct);

        var walletSummary = wallet is null
            ? new MerchantWalletSummaryDto()
            : new MerchantWalletSummaryDto
            {
                AvailableAmount = wallet.AvailableAmount,
                FrozenAmount = wallet.FrozenAmount,
                TotalDepositedAmount = wallet.TotalDepositedAmount,
            };

        // 3. 組裝詳情 DTO
        var detail = new MerchantDetailDto
        {
            MerchantId = baseDto.MerchantId,
            CompanyName = baseDto.CompanyName,
            EnglishName = baseDto.EnglishName,
            TaxId = baseDto.TaxId,
            IndustryType = baseDto.IndustryType,
            ContactName = baseDto.ContactName,
            Phone = baseDto.Phone,
            Fax = baseDto.Fax,
            CompanyEmail = baseDto.CompanyEmail,
            Website = baseDto.Website,
            Address = baseDto.Address,
            EstablishedDate = baseDto.EstablishedDate,
            OwnerEmail = baseDto.OwnerEmail,
            VerificationStatus = baseDto.VerificationStatus,
            VerifiedAt = baseDto.VerifiedAt,
            UpdatedByAdminName = baseDto.UpdatedByAdminName,
            CreatedAt = baseDto.CreatedAt,

            Contacts = contacts,
            Stats = stats,
            RecentCases = cases,
            Wallet = walletSummary,
            RecentTransactions = transactions,
            Members = members,
            RecentActivityLogs = logs,
        };

        return Result<MerchantDetailDto>.Success(detail);
    }
}
