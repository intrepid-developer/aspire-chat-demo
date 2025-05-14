namespace AspireChat.Common.Users;

public class Login
{
    public class Request
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class Response
    {
        public string? Token { get; set; }
        public bool Success { get; set; }
    }
}
