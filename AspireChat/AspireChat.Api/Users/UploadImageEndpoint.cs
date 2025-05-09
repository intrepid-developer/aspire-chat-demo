using AspireChat.Common.Users;
using Azure.Storage.Blobs;
using FastEndpoints;

namespace AspireChat.Api.Users;

public class UploadImageEndpoint(BlobServiceClient blobService) : Endpoint<UploadImage.Request, UploadImage.Response>
{
    public override void Configure()
    {
        Post("users/upload-image");
        AllowFileUploads();
        Description(x => x
            .WithName("Upload Image")
            .Produces<UploadImage.Response>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(UploadImage.Request req, CancellationToken ct)
    {
        var containerClient = blobService.GetBlobContainerClient("images");
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        var blobName = $"{Guid.NewGuid()}_{req.Image.FileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await using var stream = req.Image.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);
        
        await SendOkAsync(new UploadImage.Response(blobClient.Uri.ToString()), ct);
    }
}