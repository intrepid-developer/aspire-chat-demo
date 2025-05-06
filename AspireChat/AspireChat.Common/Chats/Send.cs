namespace AspireChat.Common.Chats;

public class Send
{
    public record Request(Guid GroupId, string Message);
    public record Response(bool Success, string? ErrorMessage = null);
}