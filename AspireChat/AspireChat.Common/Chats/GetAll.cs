namespace AspireChat.Common.Chats;

public sealed class GetAll()
{
    public record Response(List<Dto> Chats);

    public record Request;

    public record Dto(int Id, string Name);
}