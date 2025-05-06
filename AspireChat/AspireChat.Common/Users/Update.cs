namespace AspireChat.Common.Users;

public class Update
{
    public record Request(Guid Id, string Name, string Email, string? Password);
    public record Response(bool Success, string? ErrorMessage = null);
}