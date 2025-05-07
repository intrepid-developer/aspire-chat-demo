using AspireChat.Common.Groups;
using FastEndpoints;

namespace AspireChat.Api.Groups;

public class CreateEndpoint : Endpoint<Create.Request, Create.Response>
{
    public override void Configure()
    {
        Post("/groups");
        Description(x => x
            .WithName("CreateGroup")
            .Produces<Create.Response>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(Create.Request req, CancellationToken ct)
    {
        await SendOkAsync(new Create.Response(true), ct);
    }
}