namespace AspireChat.Api.Entities;

public class Group : BaseEntity
{
    public required string Name { get; set; }

    public ICollection<Chat> Chats { get; set; } = [];
}