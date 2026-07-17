using Application;
using Application.Abstractions.Security;
using Common.Primitives;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.RateLimiting;

namespace Infrastructure.Web;

public static class TaskSystemWebExtensions
{
    public static WebApplicationBuilder AddTaskSystemExternalConfiguration(this WebApplicationBuilder builder)
    {
        var externalConfig = Path.GetFullPath(
            Path.Combine(builder.Environment.ContentRootPath, "..", "Account", "TaskSystem.json"));

        builder.Configuration.AddJsonFile(externalConfig, optional: false, reloadOnChange: false);
        return builder;
    }

    public static IHostBuilder UseTaskSystemSerilog(this IHostBuilder host)
        => host.UseSerilog((context, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            if (context.Configuration.GetValue<bool>("IsDEMO"))
            {
                config.WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            }
        });

    public static IServiceCollection AddTaskSystemWeb(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        TaskSystemWebOptions options)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<TaskSystemSignInService>();

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(cookieOptions =>
            {
                cookieOptions.Cookie.Name = options.CookieName;
                cookieOptions.Cookie.HttpOnly = true;
                cookieOptions.Cookie.SecurePolicy = options.AlwaysUseSecureCookie || !environment.IsDevelopment()
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;
                cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
                cookieOptions.LoginPath = "/Account/Login";
                cookieOptions.LogoutPath = "/Account/Logout";
                cookieOptions.AccessDeniedPath = "/Account/AccessDenied";
                cookieOptions.ExpireTimeSpan = options.CookieExpireTimeSpan;
                cookieOptions.SlidingExpiration = true;
            });

        services.AddControllersWithViews(mvcOptions =>
        {
            mvcOptions.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        });

        services.AddTaskSystemRateLimiter(configuration, options.LoginPermitLimit);

        return services;
    }

    public static IServiceCollection AddTaskSystemRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration,
        int? loginPermitLimit = null)
    {
        services.AddRateLimiter(opts =>
        {
            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            opts.AddSlidingWindowLimiter(RateLimitPolicies.Login, o =>
            {
                o.PermitLimit = loginPermitLimit ?? configuration.GetValue("RateLimit:Login:PermitLimit", 5);
                o.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimit:Login:WindowSeconds", 60));
                o.SegmentsPerWindow = 3;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            opts.AddSlidingWindowLimiter(RateLimitPolicies.ForgotPassword, o =>
            {
                o.PermitLimit = configuration.GetValue("RateLimit:ForgotPassword:PermitLimit", 3);
                o.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimit:ForgotPassword:WindowSeconds", 300));
                o.SegmentsPerWindow = 3;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            opts.AddSlidingWindowLimiter(RateLimitPolicies.Global, o =>
            {
                o.PermitLimit = configuration.GetValue("RateLimit:Global:PermitLimit", 100);
                o.Window = TimeSpan.FromSeconds(configuration.GetValue("RateLimit:Global:WindowSeconds", 10));
                o.SegmentsPerWindow = 2;
                o.QueueLimit = 0;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }

    public static WebApplication UseTaskSystemWebPipeline(this WebApplication app, string defaultRoutePattern)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapControllerRoute("default", defaultRoutePattern).WithStaticAssets();

        return app;
    }
}

