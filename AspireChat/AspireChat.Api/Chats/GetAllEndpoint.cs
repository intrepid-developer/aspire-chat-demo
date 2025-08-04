using System.Security.Claims;
using AspireChat.Api.Entities;
using AspireChat.Common.Chats;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Chats;

public class GetAllEndpoint(AppDbContext db) : Endpoint<GetAll.Request, GetAll.Response>
{
    public override void Configure()
    {
        Get("/chats/{groupId}");
        Description(x => x
            .WithName("GetAllChats")
            .Produces<GetAll.Response>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status500InternalServerError));
    }

    public override async Task HandleAsync(GetAll.Request req, CancellationToken ct)
    {
        if (int.TryParse(User.FindFirst(ClaimTypes.Sid)?.Value, out var id))
        {
            var chats = await db.Database
                .SqlQuery<GetAll.Dto>($"""
                                       SELECT C.Id, 
                                              C.Name, 
                                              C.Message, 
                                              C.UserId, 
                                              U.ProfileImageUrl AS [UserAvatarUrl],
                                              CAST(CASE WHEN C.UserId = {id} THEN 1 ELSE 0 END AS BIT) AS IsMe
                                       FROM Chats C
                                       JOIN Users U ON C.UserId = U.Id
                                       WHERE GroupId = {req.GroupId}
                                       """)
                .ToListAsync(cancellationToken: ct);

            await Send.OkAsync(new GetAll.Response { Chats = chats }, ct);
        }
        else
        {
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
    }
}