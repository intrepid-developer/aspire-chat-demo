namespace AspireChat.Common.Users;

public class Register
{
    public class Request
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class Response
    {
        public string? Token { get; set; }
        public bool Success { get; set; }
    }
}
