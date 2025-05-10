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
            var response = await httpClient.PostAsJsonAsync("/users/login", request, cancellationToken);
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
            var response = await httpClient.PostAsJsonAsync("/users/register", request, cancellationToken);
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
            var response = await httpClient.PutAsJsonAsync("/users/update", request, cancellationToken);
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

    public async Task<GetProfile.Response?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue();
            var response = await httpClient.GetAsync("/users/profile", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GetProfile.Response>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "GetProfile failed");
            return null;
        }
    }

    public async Task<string?> UploadImageAsync(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue();
            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream(5 * 1024 * 1024); // 5MB limit
            content.Add(new StreamContent(stream), "Image", file.Name);
            content.Add(new StringContent(file.Name), "ImageName");
            var response = await httpClient.PostAsync("/users/upload-image", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UploadImage.Response>(cancellationToken);
            return result?.ImageUrl;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "UploadImage failed");
            return null;
        }
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string? jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        if (keyValuePairs != null)
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));

        return [];
    }
}
