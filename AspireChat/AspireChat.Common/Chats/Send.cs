namespace AspireChat.Common.Chats;

public class Send
{
    public record Request(int GroupId, string Message);
    public record Response(bool Success);
}