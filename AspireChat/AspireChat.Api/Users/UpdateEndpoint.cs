using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Users;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Users;

public class UpdateEndpoint(AppDbContext db) : Endpoint<Update.Request, Update.Response>
{
    public override void Configure()
    {
        Put("/users");
        Description(x => x
            .WithName("UpdateUser")
            .Produces<Update.Response>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(Update.Request req, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (userId is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == int.Parse(userId), ct);
        if (user is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        
        user.Email = req.Email;
        user.Name = req.Name;
        user.ProfileImageUrl = req.ProfileImageUrl;
        
        if(!string.IsNullOrEmpty(req.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        }

        await db.SaveChangesAsync(ct);

        await Send.OkAsync(new Update.Response(true), ct);
    }
}