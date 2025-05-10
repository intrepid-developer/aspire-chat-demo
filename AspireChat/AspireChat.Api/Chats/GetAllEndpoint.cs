using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Chats;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Chats;

public class GetAllEndpoint(AppDbContext db) : Endpoint<GetAll.Request, GetAll.Response>
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
        if (int.TryParse(User.FindFirst(ClaimTypes.Sid)?.Value, out var id))
        {
            var chats = await db.Chats
                .AsNoTracking()
                .Select(chat => new GetAll.Dto(chat.Id, chat.Name, chat.Message, chat.UserId))
                .ToListAsync(ct);

            await db.SaveChangesAsync(ct);

            await SendOkAsync(new GetAll.Response(chats), ct);
        }

        await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
}