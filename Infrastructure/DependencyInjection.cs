using Application.Abstractions.Notifications;
using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.AdminAccounts.Options;
using Application.FileStorage;
using Dapper;
using Infrastructure.Authentication;
using Infrastructure.FileStorage;
using Infrastructure.Notifications;
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

        // ── 郵件通知 ──────────────────────────────────────
        var emailSection = configuration.GetSection("EmailSettings");
        services.AddSingleton(new EmailSettings
        {
            Host = emailSection["Host"] ?? "smtp.gmail.com",
            Port = int.TryParse(emailSection["Port"], out var port) ? port : 587,
            FromEmail = emailSection["FromEmail"] ?? string.Empty,
            FromName = emailSection["FromName"] ?? string.Empty,
            Password = emailSection["Password"] ?? string.Empty,
            EnableSsl = !bool.TryParse(emailSection["EnableSsl"], out var enableSsl) || enableSsl,
            BaseUrl = emailSection["BaseUrl"] ?? string.Empty
        });
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddSingleton(new InvitationEmailOptions
        {
            BaseUrl = emailSection["BaseUrl"] ?? string.Empty
        });

        // ── 安全性 ────────────────────────────────────────
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // ── 資料庫連線 ────────────────────────────────────
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        // ── Unit of Work（Scoped：每個 HTTP Request 一個實例）
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositories ──────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
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
        services.AddScoped<IMerchantImpersonationTicketRepository, MerchantImpersonationTicketRepository>();
        services.AddScoped<ICaseRepository, CaseRepository>();
        services.AddScoped<ICaseAttachmentRepository, CaseAttachmentRepository>();
        services.AddScoped<ICaseBudgetSnapshotRepository, CaseBudgetSnapshotRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<ICaseFileStorage, CaseFileStorage>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<ICaseMonitorRepository, CaseMonitorRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDisputeRepository, DisputeRepository>();
        services.AddScoped<IFinanceRepository, FinanceRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<ILanguageRepository, LanguageRepository>();
        return services;
    }
}
