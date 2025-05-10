namespace AspireChat.Common.Users;

public class Register
{
    public record Request(string Name, string Email, string Password);
    public record Response(string? Token, bool Success);
}