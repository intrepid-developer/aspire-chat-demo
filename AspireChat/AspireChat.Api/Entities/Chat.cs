namespace AspireChat.Api.Entities;

public class Chat : BaseEntity
{
    public required string Message { get; set; }

    public virtual required Group Group { get; set; }
}