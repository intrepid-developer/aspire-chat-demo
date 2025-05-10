using AspireChat.Api.Entities;
using AspireChat.Common.Groups;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Groups;

public class GetAllEndpoint(AppDbContext db) : EndpointWithoutRequest<GetAll.Response>
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

    public override async Task HandleAsync(CancellationToken ct)
    {
        var groups = await db.Groups
            .AsNoTracking()
            .Select(group => new GetAll.Dto
            {
                Id = group.Id,
                Name = group.Name,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt
            })
            .ToListAsync(ct);

        await SendOkAsync(new GetAll.Response { Groups = groups }, ct);
    }
}