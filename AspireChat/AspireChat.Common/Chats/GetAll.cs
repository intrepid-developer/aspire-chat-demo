using Microsoft.AspNetCore.Mvc;

namespace AspireChat.Common.Chats;

public sealed class GetAll
{
    public class Request
    {
        [FromQuery] public int GroupId { get; set; }
    }

    public class Response
    {
        public List<Dto> Chats { get; set; } = [];
    }

    public class Dto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? UserAvatarUrl { get; set; }
        public bool IsMe { get; set; }
    }
}