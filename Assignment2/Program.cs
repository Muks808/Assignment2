using Assignment2.Data;
using Assignment2.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options => options.Filters.Add<StoreWriteFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dataPath = builder.Configuration["RaceDayStore:Path"];
builder.Services.AddSingleton(RaceDayStore.Load(dataPath));
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<AuthService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".Assignment2.Api.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDay.Auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.LoginPath = "/api/auth/login";
        options.AccessDeniedPath = "/api/auth/forbidden";
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Configuration.GetValue("HttpsRedirection:Enabled", !app.Environment.IsDevelopment()))
    app.UseHttpsRedirection();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
// The original store uses mutable lists. Serialize controller requests through response writing.
using var storeGate = new SemaphoreSlim(1);
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next(context);
        return;
    }
    await storeGate.WaitAsync(context.RequestAborted);
    try { await next(context); }
    finally { storeGate.Release(); }
});
app.MapGet("/health", () => Results.Ok()).ExcludeFromDescription();
app.MapControllers();

app.Run();

public partial class Program { }