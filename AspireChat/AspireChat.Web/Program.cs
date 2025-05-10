using AspireChat.ServiceDefaults;
using AspireChat.Web;
using AspireChat.Web.Clients;
using AspireChat.Web.Components;
using AspireChat.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Add MudBlazor Services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<AuthenticationService>();

builder.Services.AddHttpClient<ChatClient>(client => { client.BaseAddress = new("https://api"); });
builder.Services.AddHttpClient<GroupClient>(client => { client.BaseAddress = new("https://api"); });
builder.Services.AddHttpClient<UserClient>(client => { client.BaseAddress = new("https://api"); });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // 👈 redirect here when unauthorized
        options.AccessDeniedPath = "/access-denied"; // optional
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();