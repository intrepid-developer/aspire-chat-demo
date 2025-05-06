using FastEndpoints;
using FastEndpoints.Security;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services
    .AddAuthenticationJwtBearer(s =>
        s.SigningKey = builder.Configuration.GetValue<string>("JWT_KEY")
    )
    .AddAuthorization()
    .AddFastEndpoints();
builder.Services.Configure<JwtCreationOptions>(o =>
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

app.Run();