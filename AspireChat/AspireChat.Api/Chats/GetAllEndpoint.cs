using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Chats;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Chats;

[Authorize]
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
            var chats = await (
                from c in db.Chats.AsNoTracking()
                join u in db.Users.AsNoTracking() on c.UserId equals u.Id
                where c.Group.Id == req.GroupId
                select new GetAll.Dto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Message = c.Message,
                    UserId = c.UserId,
                    UserAvatarUrl = u.ProfileImageUrl,
                    IsMe = c.UserId == id
                }
            ).ToListAsync(ct);

            await Send.OkAsync(new GetAll.Response { Chats = chats }, ct);
        }
        else
        {
            await Send.UnauthorizedAsync(ct);
        }
    }
}