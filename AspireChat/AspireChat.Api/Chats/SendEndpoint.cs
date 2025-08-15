using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Api.Hubs;
using AspireChat.Common.Chats;
using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Chats;

public class SendEndpoint(AppDbContext db, IHubContext<GroupChatHub> hubContext) : Endpoint<Send.Request, Send.Response>
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
            var chat = new Chat
            {
                Message = req.Message,
                Name = user.Name,
                UserId = user.Id,
                Group = group
            };
            group.Chats.Add(chat);

            await db.SaveChangesAsync(ct);

            // Broadcast the new message to all clients in this group via SignalR
            var dto = new GetAll.Dto
            {
                Id = chat.Id,
                Name = chat.Name,
                Message = chat.Message,
                UserId = chat.UserId,
                UserAvatarUrl = user.ProfileImageUrl,
                IsMe = false
            };

            await hubContext.Clients.Group(req.GroupId.ToString())
                .SendAsync("ReceiveMessage", dto, cancellationToken: ct);

            await Send.OkAsync(new Send.Response
            {
                Success = true
            }, ct);
        }
        else
        {
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
    }
}