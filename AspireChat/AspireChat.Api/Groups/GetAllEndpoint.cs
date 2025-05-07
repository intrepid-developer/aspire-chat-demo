using AspireChat.Api.Entities;
using AspireChat.Common.Chats;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Groups;

public class GetAllEndpoint(AppDbContext db) : Endpoint<GetAll.Request, GetAll.Response>
{
    public override void Configure()
    {
        Get("/groups");
        Description(x => x
            .WithName("GetAllGroups")
            .Produces<GetAll.Response>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(GetAll.Request req, CancellationToken ct)
    {
        var groups = await db.Groups
            .AsNoTracking()
            .Select(group => new GetAll.Dto(group.Id, group.Name))
            .ToListAsync(ct);
        
        await SendOkAsync(new GetAll.Response(groups), ct);
    }
}