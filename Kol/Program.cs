using Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddTaskSystemExternalConfiguration();
builder.Host.UseTaskSystemSerilog();

builder.Services.AddTaskSystemWeb(
    builder.Configuration,
    builder.Environment,
    new TaskSystemWebOptions
    {
        CookieName = ".TaskSystem.Kol",
        CookieExpireTimeSpan = TimeSpan.FromMinutes(480),
        AlwaysUseSecureCookie = true
    });

var app = builder.Build();

app.UseTaskSystemWebPipeline("{controller=Home}/{action=Index}/{id?}");

app.Run();
