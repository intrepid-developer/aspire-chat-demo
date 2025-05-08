namespace AspireChat.Common.Groups;

public sealed class GetAll
{
    public record Response(List<Dto> Groups);

    public record Request;

    public record Dto(Guid Id, string Name, DateTime CreatedAt, DateTime UpdatedAt);
}