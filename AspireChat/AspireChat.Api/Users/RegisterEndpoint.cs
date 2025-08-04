using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Users;
using FastEndpoints;
using FastEndpoints.Security;

namespace AspireChat.Api.Users;

public class RegisterEndpoint(AppDbContext db) : Endpoint<Register.Request, Register.Response>
{
    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
        Description(x => x
            .Produces<Register.Response>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(Register.Request req, CancellationToken ct)
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var user = new User
        {
            Name = req.Name,
            Email = req.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await db.Users.AddAsync(user, ct);

        await db.SaveChangesAsync(ct);

        var token = JwtBearer.CreateToken(o =>
        {
            o.ExpireAt = DateTime.UtcNow.AddHours(24);
            o.User.Claims.AddRange([
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Sid, user.Id.ToString())
            ]);
        });

        await Send.OkAsync(new Register.Response
        {
            Token = token,
            Success = true
        }, cancellation: ct);
    }
}