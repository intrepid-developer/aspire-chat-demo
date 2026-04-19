using Microsoft.AspNetCore.Http;

namespace AspireChat.Common.Users;

public class UploadImage
{
    public class Request
    {
        public IFormFile Image { get; set; } = null!;
        public string ImageName { get; set; } = string.Empty;
    }
    public class Response
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
