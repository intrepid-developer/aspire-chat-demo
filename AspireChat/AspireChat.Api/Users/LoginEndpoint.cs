using AspireChat.Common.Users;
using FastEndpoints;
using FastEndpoints.Security;

namespace AspireChat.Api.Users;

public class LoginEndpoint : Endpoint<Login.Request, Login.Response>
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
        var token = JwtBearer.CreateToken(o =>
        {
            o.ExpireAt = DateTime.UtcNow.AddHours(1);
            o.User.Claims.Add(("UserId", "001"));
        });

        await SendAsync(new Login.Response(
            Guid.NewGuid(),
            token,
            true
        ), cancellation: ct);
    }
}