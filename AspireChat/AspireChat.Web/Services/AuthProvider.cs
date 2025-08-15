using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AspireChat.Web.Services;

public class AuthProvider(ProtectedSessionStorage sessionStorage) : AuthenticationStateProvider
{
    public async Task<AuthenticationHeaderValue> AuthorizationHeaderValue()
    {
        var token = await sessionStorage.GetAsync<string>("token");
        return new AuthenticationHeaderValue("Bearer", token.Value);
    }

    // Helper to get current user id from the stored JWT (supports multiple claim type keys)
    public async Task<int?> GetUserIdAsync()
    {
        var token = await sessionStorage.GetAsync<string>("token");
        var raw = token.Value;
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var claims = ParseClaimsFromJwt(raw).ToList();

        // Possible keys for user id depending on how the JWT was created/serialized
        string[] keys =
        [
            "sid",
            System.Security.Claims.ClaimTypes.Sid, // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid"
            "nameid",
            System.Security.Claims.ClaimTypes.NameIdentifier, // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
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
        return null;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var anonymous = new ClaimsIdentity();
        var anonymousUser = new ClaimsPrincipal(anonymous);
        return Task.FromResult(new AuthenticationState(anonymousUser));
    }

    public async Task AuthenticateUser(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var user = new ClaimsPrincipal(identity);

        await sessionStorage.SetAsync("token", token);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(user)));
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