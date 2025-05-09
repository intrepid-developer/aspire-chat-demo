using AspireChat.Common.Users;
using AspireChat.Web.Services;

namespace AspireChat.Web.Clients;

public class UserClient(AuthenticationService authService, HttpClient httpClient, ILogger<UserClient> logger)
{
    public async Task<bool> LoginAsync(Login.Request request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/login", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Login.Response>(cancellationToken);
            authService.Token = content?.Token ?? string.Empty;
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Login failed");
            return false;
        }
    }

    public async Task<bool> RegisterAsync(Register.Request request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/register", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Register.Response>(cancellationToken);
            authService.Token = content?.Token ?? string.Empty;
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Register failed");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Update.Request request, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue;
            var response = await httpClient.PutAsJsonAsync("/update", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Update.Response>(cancellationToken);
            return content?.Success ?? false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Update failed");
            return false;
        }
    }
}