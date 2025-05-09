namespace AspireChat.Common.Users;

public class GetProfile
{
    public record Request;

    public record Response(string Name, string Email, string? ProfileImageUrl, DateTime CreatedAt, DateTime UpdatedAt);
}