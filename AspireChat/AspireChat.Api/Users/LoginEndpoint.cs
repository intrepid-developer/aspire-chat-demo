using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Users;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Users;

public class LoginEndpoint(AppDbContext db) : Endpoint<Login.Request, Login.Response>
{
    public override void Configure()
    {
        Post("/users/login");
        AllowAnonymous();
        Description(x => x
            .WithName("Login")
            .Produces<Login.Response>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(Login.Request req, CancellationToken ct)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(x => x.Email == req.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var token = JwtBearer.CreateToken(o =>
        {
            o.ExpireAt = DateTime.UtcNow.AddHours(24);
            o.User.Claims.AddRange([
                new Claim(ClaimTypes.Email, user!.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Sid, user.Id.ToString())
            ]);
        });

        await Send.OkAsync(new Login.Response{
            Token = token,
            Success = true
        }, cancellation: ct);
    }
}