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

app.UseTaskSystemWebPipeline("{controller=Account}/{action=Login}/{id?}");

app.Run();
