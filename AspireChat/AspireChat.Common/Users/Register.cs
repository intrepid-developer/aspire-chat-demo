namespace AspireChat.Common.Users;

public class Register
{
    public record Request(string Name, string Email, string Password);
    public record Response(Guid Id, string Token, bool Success, string? ErrorMessage = null);
}