using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Chats;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Chats;

public class SendEndpoint(AppDbContext db) : Endpoint<Send.Request, Send.Response>
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
        if (int.TryParse(User.FindFirst(ClaimTypes.Sid)?.Value, out var id))
        {
            var user = await db.Users.FirstAsync(x => x.Id == id, ct);
            var group = await db.Groups
                .FirstAsync(x => x.Id == req.GroupId, ct);
            user.Chats.Add(new Chat
            {
                Message = req.Message,
                CreatedById = id,
                CreatedBy = user,
                Group = group
            });

            await db.SaveChangesAsync(ct);

            await SendOkAsync(new Send.Response(true), ct);
        }

        await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
}