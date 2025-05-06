namespace AspireChat.Common.Chats;

public sealed class GetAll()
{
    public record Response(List<Dto> Chats);

    public record Request(Guid GroupId);

    public record Dto(Guid UserId, int Id, string Message, DateTime CreatedAt, DateTime UpdatedAt);
}