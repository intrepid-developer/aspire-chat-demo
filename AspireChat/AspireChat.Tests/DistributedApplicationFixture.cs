using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace AspireChat.Tests;

public sealed class DistributedApplicationFixture : IAsyncLifetime
{
    private const string JwtKey = "aspire-chat-integration-test-jwt-key-1234567890";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
    private string? previousJwtKey;

    public DistributedApplication App { get; private set; } = null!;
    public string RunPrefix { get; } = $"itest-{Guid.NewGuid():N}";

    public async ValueTask InitializeAsync()
    {
        previousJwtKey = Environment.GetEnvironmentVariable("Parameters__jwt_key");
        Environment.SetEnvironmentVariable("Parameters__jwt_key", JwtKey);

        using var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireChat_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Information);
            logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            logging.AddFilter("Aspire.", LogLevel.Warning);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder => clientBuilder.AddStandardResilienceHandler());

        App = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await App.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("api", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("web", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (App is not null)
        {
            await App.DisposeAsync();
        }

        Environment.SetEnvironmentVariable("Parameters__jwt_key", previousJwtKey);
    }

    public HttpClient CreateApiClient() => App.CreateHttpClient("api");

    public HttpClient CreateWebClient() => App.CreateHttpClient("web");
}
