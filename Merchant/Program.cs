using Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddTaskSystemExternalConfiguration();
builder.Host.UseTaskSystemSerilog();

builder.Services.AddTaskSystemWeb(
    builder.Configuration,
    builder.Environment,
    new TaskSystemWebOptions
    {
        CookieName = ".TaskSystem.Merchant",
        CookieExpireTimeSpan = TimeSpan.FromMinutes(60)
    });

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

// 管理者代理登入業者端的唯讀強制 Middleware（必須在 Authentication 之後、Controller 之前）
app.UseMiddleware<ImpersonationReadOnlyMiddleware>();

app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Account}/{action=Login}/{id?}").WithStaticAssets();

app.Run();
