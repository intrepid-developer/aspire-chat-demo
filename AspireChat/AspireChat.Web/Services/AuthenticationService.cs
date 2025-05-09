using System.Net.Http.Headers;

namespace AspireChat.Web.Services;

public class AuthenticationService
{
    public string Token { get; set; } = string.Empty;
    
    public AuthenticationHeaderValue? AuthorizationHeaderValue => 
        string.IsNullOrEmpty(Token) ? null : new AuthenticationHeaderValue("Bearer", Token);
}