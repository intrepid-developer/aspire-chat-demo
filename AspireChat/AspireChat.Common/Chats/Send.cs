namespace AspireChat.Common.Chats;

public class Send
{
    public class Request
    {
        public int GroupId { get; set; }
        public string Message { get; set; }
        public Request() { }
    }
    public class Response
    {
        public bool Success { get; set; }
        public Response() { }
    }
}
