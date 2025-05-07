using AspireChat.Common.Users;
using FastEndpoints;

namespace AspireChat.Api.Users;

public class GetProfileEndpoint : Endpoint<GetProfile.Request, GetProfile.Response>
{
    public override void Configure()
    {
        Get("/users/profile");
        Description(x => x
            .Produces<Register.Response>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(GetProfile.Request req, CancellationToken ct)
    {
        await SendOkAsync(
            new GetProfile.Response(Guid.NewGuid(), string.Empty, string.Empty, DateTime.Now, DateTime.Now), ct);
    }
}