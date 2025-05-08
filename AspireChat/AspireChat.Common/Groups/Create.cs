namespace AspireChat.Common.Groups;

public class Create
{
    public record Request(string Name);
    public record Response(bool Success);
}