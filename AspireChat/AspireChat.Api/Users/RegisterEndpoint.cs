using AspireChat.Common.Users;
using FastEndpoints;
using FastEndpoints.Security;

namespace AspireChat.Api.Users;

public class RegisterEndpoint : Endpoint<Register.Request, Register.Response>
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
        // Simulate user registration logic
        await Task.Delay(1000, ct); // Simulate async work

        var token = JwtBearer.CreateToken(o =>
        {
            o.ExpireAt = DateTime.UtcNow.AddHours(1);
            o.User.Claims.Add(("UserId", "001"));
        });
        
        // Simulate successful registration
        var userId = Guid.NewGuid();
        var response = new Register.Response(userId, token, true);

        await SendAsync(response, cancellation: ct);
    }
}