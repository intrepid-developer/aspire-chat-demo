using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Groups;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Group = AspireChat.Api.Entities.Group;

namespace AspireChat.Api.Groups;

public class CreateEndpoint(AppDbContext db) : Endpoint<Create.Request, Create.Response>
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
        if (int.TryParse(User.FindFirst(ClaimTypes.Sid)?.Value, out var id))
        {
            var user = await db.Users.FirstAsync(x => x.Id == id, ct);
            user.Groups.Add(new Group
            {
                Name = req.Name,
                CreatedById = id,
                CreatedBy = user
            });
            
            await db.SaveChangesAsync(ct);
            
            await SendOkAsync(new Create.Response(true), ct);
        }

        await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
}