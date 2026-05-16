using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AspireChat.Web.Services;

public class AuthProvider(ProtectedSessionStorage sessionStorage) : AuthenticationStateProvider
{
    public async Task<AuthenticationHeaderValue?> AuthorizationHeaderValue()
    {
        var tokenResult = await sessionStorage.GetAsync<string>("token");
        if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Value))
        {
            return null;
        }
        return new AuthenticationHeaderValue("Bearer", tokenResult.Value);
    }

    // Helper to get current user id from the stored JWT (supports multiple claim type keys)
    public async Task<int?> GetUserIdAsync()
    {
        var tokenResult = await sessionStorage.GetAsync<string>("token");
        var raw = tokenResult.Success ? tokenResult.Value : null;
        if (string.IsNullOrWhiteSpace(raw)) return null;

        try
        {
            var claims = ParseClaimsFromJwt(raw).ToList();

            // Possible keys for user id depending on how the JWT was created/serialized
            string[] keys =
            [
                "sid",
                System.Security.Claims.ClaimTypes.Sid,
                "nameid",
                System.Security.Claims.ClaimTypes.NameIdentifier,
                "sub",
                "userid",
                "userId",
                "id"
            ];

            string? value = null;
            foreach (var key in keys)
            {
                var match = claims.FirstOrDefault(c => string.Equals(c.Type, key, StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrWhiteSpace(match))
                {
                    value = match;
                    break;
                }
            }

            if (int.TryParse(value, out var id)) return id;
        }
        catch
        {
            // Malformed token - treat as unauthenticated
        }

        return null;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var tokenResult = await sessionStorage.GetAsync<string>("token");
            var token = tokenResult.Success ? tokenResult.Value : null;

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // Invalid or expired token in storage - return anonymous
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task AuthenticateUser(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            await sessionStorage.DeleteAsync("token");
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
            return;
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var user = new ClaimsPrincipal(identity);

        await sessionStorage.SetAsync("token", token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return [];

        var payload = parts[1];
        var jsonBytes = Convert.FromBase64String(payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '='));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        if (keyValuePairs != null)
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));

        return [];
    }
}