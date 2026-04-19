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

        App = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await App.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("sql", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("db", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("api", cancellationToken)
            .WaitAsync(DefaultTimeout, cancellationToken);
        await WaitForApiReadyAsync(cancellationToken);
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

    private async Task WaitForApiReadyAsync(CancellationToken cancellationToken)
    {
        using var apiClient = App.CreateHttpClient("api");
        apiClient.Timeout = TimeSpan.FromSeconds(10);

        var deadline = DateTime.UtcNow.Add(DefaultTimeout);
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                foreach (var probePath in new[] { "/health", "/users/profile", "/" })
                {
                    using var response = await apiClient.GetAsync(probePath, cancellationToken);
                    if (response is null)
                    {
                        continue;
                    }

                    if ((int)response.StatusCode < 500)
                    {
                        return;
                    }

                    lastError = new InvalidOperationException($"API readiness probe hit {probePath} but returned status {(int)response.StatusCode}.");
                }

                if (lastError is null)
                {
                    lastError = new InvalidOperationException("API readiness probe did not return an HTTP response.");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
            }

            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException("API readiness probe did not return a successful health response in time.", lastError);
    }
}
