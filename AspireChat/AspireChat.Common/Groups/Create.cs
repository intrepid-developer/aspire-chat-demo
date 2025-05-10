namespace AspireChat.Common.Groups;

public class Create
{
    public class Request
    {
        public string Name { get; set; }
        public Request() { }
    }
    public class Response
    {
        public bool Success { get; set; }
        public Response() { }
    }
}
