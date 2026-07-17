using Infrastructure.Web;

var builder = WebApplication.CreateBuilder(args);

builder.AddTaskSystemExternalConfiguration();
builder.Host.UseTaskSystemSerilog();

builder.Services.AddTaskSystemWeb(
    builder.Configuration,
    builder.Environment,
    new TaskSystemWebOptions
    {
        CookieName = ".TaskSystem.Admin",
        CookieExpireTimeSpan = TimeSpan.FromMinutes(60),
        LoginPermitLimit = 3
    });

var app = builder.Build();

app.UseTaskSystemWebPipeline("{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
