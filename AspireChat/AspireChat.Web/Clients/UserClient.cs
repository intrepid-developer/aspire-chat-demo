using System.Security.Claims;
using System.Text.Json;
using AspireChat.Common.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AuthenticationService = AspireChat.Web.Services.AuthenticationService;

namespace AspireChat.Web.Clients;

public class UserClient(
    AuthenticationService authService,
    HttpClient httpClient,
    ILogger<UserClient> logger,
    IHttpContextAccessor contextAccessor)
{
    public async Task<bool> LoginAsync(Login.Request request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/login", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Login.Response>(cancellationToken);
            authService.Token = content?.Token ?? string.Empty;

            await SignIn();

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Login failed");
            return false;
        }
    }

    private async Task SignIn()
    {
        var claims = ParseClaimsFromJwt(authService.Token);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await contextAccessor.HttpContext!.SignInAsync(principal);

        // Optionally store token for future API calls
        contextAccessor.HttpContext!.Response.Cookies.Append("access_token", authService.Token);
    }

    public async Task<bool> RegisterAsync(Register.Request request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/register", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Register.Response>(cancellationToken);
            authService.Token = content?.Token ?? string.Empty;

            await SignIn();

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
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue();
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

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        if (keyValuePairs != null)
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));

        return [];
    }
}