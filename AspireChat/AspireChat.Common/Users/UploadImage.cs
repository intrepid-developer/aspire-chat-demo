using Microsoft.AspNetCore.Http;

namespace AspireChat.Common.Users;

public class UploadImage
{
    public record Request(IFormFile Image, string ImageName);
    public record Response(string ImageUrl);
}