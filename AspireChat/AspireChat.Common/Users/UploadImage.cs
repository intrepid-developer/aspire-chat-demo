using Microsoft.AspNetCore.Http;

namespace AspireChat.Common.Users;

public class UploadImage
{
    public class Request
    {
        public IFormFile Image { get; set; }
        public string ImageName { get; set; }
    }
    public class Response
    {
        public string ImageUrl { get; set; }
    }
}
