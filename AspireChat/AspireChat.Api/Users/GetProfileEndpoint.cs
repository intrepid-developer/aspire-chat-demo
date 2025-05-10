using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Users;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Users;

public class GetProfileEndpoint(AppDbContext db) : EndpointWithoutRequest<GetProfile.Response>
{
    public override void Configure()
    {
        Get("/users/profile");
        Description(x => x
            .Produces<Register.Response>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (userId is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == int.Parse(userId), ct);

        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(
            new GetProfile.Response
            {
                Name = user.Name,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            }, ct);
    }
}