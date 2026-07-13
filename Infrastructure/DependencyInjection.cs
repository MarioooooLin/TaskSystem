using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Dapper;
using Infrastructure.Authentication;
using Infrastructure.Persistence.Dapper;
using Infrastructure.Persistence.Dapper.TypeHandlers;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Transactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Dapper TypeHandlers（全域，只需註冊一次）─────────
        SqlMapper.AddTypeHandler(DateOnlyTypeHandler.Instance);

        // ── 安全性 ────────────────────────────────────────
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // ── 資料庫連線 ────────────────────────────────────
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        // ── Unit of Work（Scoped：每個 HTTP Request 一個實例）
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositories ──────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        services.AddScoped<IMerchantContactRepository, MerchantContactRepository>();
        services.AddScoped<IMerchantStatsRepository, MerchantStatsRepository>();
        services.AddScoped<IMerchantMemberRepository, MerchantMemberRepository>();
        services.AddScoped<IMerchantWalletRepository, MerchantWalletRepository>();
        services.AddScoped<IMerchantCreditWalletRepository, MerchantCreditWalletRepository>();
        services.AddScoped<IKolRepository, KolRepository>();
        services.AddScoped<IKolSocialAccountRepository, KolSocialAccountRepository>();
        services.AddScoped<IKolBankAccountRepository, KolBankAccountRepository>();
        services.AddScoped<IKolStatsRepository, KolStatsRepository>();
        services.AddScoped<IKolEarningRepository, KolEarningRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<ICaseMonitorRepository, CaseMonitorRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDisputeRepository, DisputeRepository>();
        services.AddScoped<IFinanceRepository, FinanceRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        return services;
    }
}
