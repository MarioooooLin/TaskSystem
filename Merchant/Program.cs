using Application;
using Application.Abstractions.Security;
using Common.Primitives;
using Infrastructure;
using Merchant.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── 外部設定檔（不進版控）────────────────────────────────
var externalConfig = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "Account", "TaskSystem.json"));
builder.Configuration.AddJsonFile(externalConfig, optional: false, reloadOnChange: false);

// ── Serilog ──────────────────────────────────────────────
builder.Host.UseSerilog((context, config) =>
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

// ── DI ───────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// ── Cookie 驗證（只允許 AccountType = Merchant）──────────
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".TaskSystem.Merchant";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(480);
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews(options =>
{
    // CSRF：所有 POST / PUT / DELETE 自動驗證 AntiForgeryToken
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// ── Rate Limiting ─────────────────────────────────────────
var cfg = builder.Configuration;
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opts.AddSlidingWindowLimiter(RateLimitPolicies.Login, o =>
    {
        o.PermitLimit = cfg.GetValue("RateLimit:Login:PermitLimit", 5);
        o.Window = TimeSpan.FromSeconds(cfg.GetValue("RateLimit:Login:WindowSeconds", 60));
        o.SegmentsPerWindow = 3;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    opts.AddSlidingWindowLimiter(RateLimitPolicies.ForgotPassword, o =>
    {
        o.PermitLimit = cfg.GetValue("RateLimit:ForgotPassword:PermitLimit", 3);
        o.Window = TimeSpan.FromSeconds(cfg.GetValue("RateLimit:ForgotPassword:WindowSeconds", 300));
        o.SegmentsPerWindow = 3;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    opts.AddSlidingWindowLimiter(RateLimitPolicies.Global, o =>
    {
        o.PermitLimit = cfg.GetValue("RateLimit:Global:PermitLimit", 100);
        o.Window = TimeSpan.FromSeconds(cfg.GetValue("RateLimit:Global:WindowSeconds", 10));
        o.SegmentsPerWindow = 2;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// ────────────────────────────────────────────────────────
var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
