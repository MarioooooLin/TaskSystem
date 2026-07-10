using Application.Account;
using Application.Cases.Queries;
using Application.Dashboard.Queries;
using Application.Kols.Commands;
using Application.Kols.Queries;
using Application.Merchants.Commands;
using Application.Merchants.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── Account ───────────────────────────────────────
        services.AddScoped<LoginHandler>();

        // ── Merchant Management ───────────────────────────
        services.AddScoped<GetMerchantListHandler>(); services.AddScoped<GetMerchantSummaryHandler>(); services.AddScoped<GetMerchantDetailHandler>();
        services.AddScoped<SuspendMerchantHandler>();
        services.AddScoped<UnsuspendMerchantHandler>();
        services.AddScoped<UpdateMerchantHandler>();
        services.AddScoped<AddMerchantContactHandler>();
        services.AddScoped<UpdateMerchantContactHandler>();
        services.AddScoped<RemoveMerchantContactHandler>();
        services.AddScoped<AdjustMerchantCreditHandler>();

        // ── Case Monitor ──────────────────────────────────
        services.AddScoped<GetCaseListHandler>();
        services.AddScoped<GetCaseSummaryHandler>();
        services.AddScoped<GetCaseDetailHandler>();

        // ── Dashboard ─────────────────────────────────────
        services.AddScoped<GetDashboardHandler>();

        // ── Dispute ───────────────────────────────────────
        services.AddScoped<GetDisputeListHandler>();
        services.AddScoped<GetDisputeSummaryHandler>();

        // ── KOL Management ────────────────────────────────
        services.AddScoped<GetKolListHandler>();
        services.AddScoped<GetKolSummaryHandler>();
        services.AddScoped<GetKolReviewListHandler>();
        services.AddScoped<GetKolReviewSummaryHandler>();
        services.AddScoped<GetKolDetailHandler>();
        services.AddScoped<ApproveKolHandler>();
        services.AddScoped<RejectKolHandler>();
        services.AddScoped<SuspendKolHandler>();
        services.AddScoped<UnsuspendKolHandler>();

        return services;
    }
}
