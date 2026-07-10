using Admin.Extensions;
using Application;
using Application.Abstractions.Security;
using Common.Primitives;
using Infrastructure;
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

// ── Cookie 驗證（只允許 AccountType = Admin，Session 較短）
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".TaskSystem.Admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest   // 開發時相容 HTTP
            : CookieSecurePolicy.Always;          // 正式環境強制 HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;     // 開發時用 Lax，避免重導向後 Cookie 遺失；正式機可改回 Strict
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Admin Session 較短
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews(options =>
{
    // CSRF：所有 POST / PUT / DELETE 自動驗證 AntiForgeryToken
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// ── Rate Limiting（Admin 門檻更嚴）────────────────────────
var cfg = builder.Configuration;
builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Admin 登入：60 秒內最多 3 次（比 Kol/Merchant 更嚴）
    opts.AddSlidingWindowLimiter(RateLimitPolicies.Login, o =>
    {
        o.PermitLimit = 3;
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
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
