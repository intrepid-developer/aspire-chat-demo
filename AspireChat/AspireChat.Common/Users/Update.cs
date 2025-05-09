namespace AspireChat.Common.Users;

public class Update
{
    public record Request(string Name, string Email, string? ProfileImageUrl, string? Password);
    public record Response(bool Success);
}