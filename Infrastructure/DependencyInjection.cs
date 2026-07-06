using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Infrastructure.Authentication;
using Infrastructure.Persistence.Dapper;
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
        // ── 安全性 ────────────────────────────────────────
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // ── 資料庫連線 ────────────────────────────────────
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        // ── Unit of Work（Scoped：每個 HTTP Request 一個實例）
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Repositories ──────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }
}
