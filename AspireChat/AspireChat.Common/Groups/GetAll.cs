namespace AspireChat.Common.Groups;

public sealed class GetAll
{
    public class Response
    {
        public List<Dto> Groups { get; set; } = [];
    }

    public class Request
    {
    }

    public class Dto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
