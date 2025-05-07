using AspireChat.Common.Chats;
using FastEndpoints;

namespace AspireChat.Api.Chats;

public class SendEndpoint : Endpoint<Send.Request, Send.Response>
{
    public override void Configure()
    {
        Post("/chats/{groupId}");
        Description(x => x
            .WithName("SendChatMessage")
            .Produces<Send.Response>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(Send.Request req, CancellationToken ct)
    {
        await SendOkAsync(new Send.Response(true), ct);
    }
}