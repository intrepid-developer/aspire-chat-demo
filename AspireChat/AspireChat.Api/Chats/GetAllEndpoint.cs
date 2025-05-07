using AspireChat.Common.Chats;
using FastEndpoints;

namespace AspireChat.Api.Chats;

public class GetAllEndpoint : Endpoint<GetAll.Request, GetAll.Response>
{
    public override void Configure()
    {
        Get("/chats/{groupId}");
        Description(x => x
            .WithName("GetAllChats")
            .Produces<GetAll.Response>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(GetAll.Request req, CancellationToken ct)
    {
        await SendOkAsync(new GetAll.Response([]), ct);
    }
}