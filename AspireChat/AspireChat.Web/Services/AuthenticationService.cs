using System.Net.Http.Headers;

namespace AspireChat.Web.Services;

public class AuthenticationService(IHttpContextAccessor httpContextAccessor)
{
    public string? Token { get; set; } = string.Empty;

    public AuthenticationHeaderValue? AuthorizationHeaderValue()
    {
        Token = httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
        return new AuthenticationHeaderValue("Bearer", Token);
    }
}