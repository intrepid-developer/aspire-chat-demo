namespace AspireChat.Common.Users;

public class Login
{
    public record Request(string Email, string Password);
    public record Response(string Token, bool Success, string? ErrorMessage = null);
}