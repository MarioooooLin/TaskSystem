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

        // 2. 平行查詢子集合（全為唯讀，不需要 Transaction 一致性）
        var contactsTask = contactRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var statsTask = statsRepo.GetStatsByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var casesTask = statsRepo.GetRecentCasesAsync(query.MerchantId, RecentCount, uow.Session, ct);
        var walletTask = walletRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var txTask = walletRepo.GetRecentTransactionsAsync(query.MerchantId, RecentCount, uow.Session, ct);
        var membersTask = memberRepo.GetMemberListAsync(query.MerchantId, uow.Session, ct);
        var logsTask = statsRepo.GetRecentActivityLogsAsync(query.MerchantId, RecentCount, uow.Session, ct);

        await Task.WhenAll(contactsTask, statsTask, casesTask, walletTask, txTask, membersTask, logsTask);

        var wallet = await walletTask;
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

            Contacts = await contactsTask,
            Stats = await statsTask,
            RecentCases = await casesTask,
            Wallet = walletSummary,
            RecentTransactions = await txTask,
            Members = await membersTask,
            RecentActivityLogs = await logsTask,
        };

        return Result<MerchantDetailDto>.Success(detail);
    }
}
