using Aspire.Hosting.Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AspireChat.AppHost.Commands;

public static class BlobCommands
{
    public static IResourceBuilder<AzureBlobStorageResource> WithSeedCommand(
        this IResourceBuilder<AzureBlobStorageResource> builder)
    {
        builder.WithCommand(
            "seed",
            "Seed the blob storage with images",
            context => OnRunSeedCommandAsync(builder, context),
            commandOptions: new CommandOptions
            {
                UpdateState = OnUpdateResourceState,
                IconName = "ArrowUpload",
                IconVariant = IconVariant.Filled
            }
        );

        return builder;
    }

    private static async Task<ExecuteCommandResult> OnRunSeedCommandAsync(
        IResourceBuilder<AzureBlobStorageResource> builder,
        ExecuteCommandContext context)
    {
        var connectionString = await builder.Resource.ConnectionStringExpression.GetValueAsync(context.CancellationToken);
        var blobStorageClient = new BlobServiceClient(connectionString);
        var containerClient = blobStorageClient.GetBlobContainerClient("static-assets");
        await containerClient.CreateIfNotExistsAsync(
            publicAccessType: PublicAccessType.Blob,
            cancellationToken: context.CancellationToken);
        
        // Get all images from the static-assets folder
        var images = Directory.GetFiles("SeedData/static-assets", "*.*", SearchOption.AllDirectories);
        foreach (var image in images)
        {
            var blobName = Path.GetFileName(image);
            var blobClient = containerClient.GetBlobClient(blobName);
            await using var stream = File.OpenRead(image);
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: context.CancellationToken);
        }
        return CommandResults.Success();
    }

    private static ResourceCommandState OnUpdateResourceState(
        UpdateCommandStateContext context)
    {
        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }
}