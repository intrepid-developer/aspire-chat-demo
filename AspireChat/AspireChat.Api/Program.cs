using AspireChat.Api.Entities;
using AspireChat.ServiceDefaults;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add SignalR
builder.Services.AddSignalR();

// Add Entity Framework Core with SQL Server
builder.AddSqlServerDbContext<AppDbContext>("db");

// Add Blob Storage
builder.AddAzureBlobServiceClient("blobs");

// Add FastEndpoints + JWT Authentication
builder.Services.AddFastEndpoints();

var jwtKey = builder.Configuration.GetValue<string>("JWT_KEY")
    ?? throw new ArgumentNullException("JWT_KEY", "JWT_KEY configuration is missing. This must be provided as a secret parameter when running under Aspire.");

builder.Services
    .AddAuthenticationJwtBearer(
        signingOptions =>
        {
            signingOptions.SigningKey = jwtKey;
        },
        jwtBearerOptions =>
        {
            // Allow SignalR clients to pass the JWT via 'access_token' query string parameter
            // (necessary for WebSocket and Server-Sent Events transports)
            jwtBearerOptions.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        })
    .AddAuthorization()
    .AddFastEndpoints();

// Configure token creation options used by JwtBearer.CreateToken() in Login/Register endpoints
builder.Services.Configure<JwtCreationOptions>(o => o.SigningKey = jwtKey);

// Also configure JwtSigningOptions explicitly (defense in depth)
builder.Services.Configure<JwtSigningOptions>(o => o.SigningKey = jwtKey);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

// Map SignalR hubs (require authenticated users)
app.MapHub<AspireChat.Api.Hubs.GroupChatHub>("/hubs/groupchat").RequireAuthorization();

app.MapDefaultEndpoints();

// Apply migrations and create the database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();