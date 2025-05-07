namespace AspireChat.Common.Users;

public class GetProfile
{
    public record Request;
    public record Response(Guid Id, string Name, string Email, DateTime CreatedAt, DateTime UpdatedAt);
}