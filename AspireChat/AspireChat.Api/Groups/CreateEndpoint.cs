using AspireChat.Api.Entities;
using AspireChat.Common.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Group = AspireChat.Api.Entities.Group;

namespace AspireChat.Api.Groups;

[Authorize]
public class CreateEndpoint(AppDbContext db) : Endpoint<Create.Request, Create.Response>
{
    public override void Configure()
    {
        Post("/groups");
        Description(x => x
            .WithName("CreateGroup")
            .Produces<Create.Response>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(Create.Request req, CancellationToken ct)
    {
        await db.Groups.AddAsync(new Group
        {
            Name = req.Name
        }, ct);

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(new Create.Response
        {
            Success = true
        }, ct);
    }
}