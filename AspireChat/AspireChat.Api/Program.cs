using AspireChat.Api.Entities;
using AspireChat.ServiceDefaults;
using FastEndpoints;
using FastEndpoints.Security;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Entity Framework Core with SQL Server
builder.AddSqlServerDbContext<AppDbContext>("db");

// Add Blob Storage
builder.AddAzureBlobServiceClient("blobs");

// Add FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services
    .AddAuthenticationJwtBearer(_ => { })
    .AddAuthorization()
    .AddFastEndpoints();
builder.Services.Configure<JwtCreationOptions>(o =>
    o.SigningKey = builder.Configuration.GetValue<string>("JWT_KEY") ?? throw new ArgumentNullException()
);
builder.Services.Configure<JwtSigningOptions>(o =>
    o.SigningKey = builder.Configuration.GetValue<string>("JWT_KEY") ?? throw new ArgumentNullException()
);

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

app.MapDefaultEndpoints();

// Apply migrations and create the database if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();