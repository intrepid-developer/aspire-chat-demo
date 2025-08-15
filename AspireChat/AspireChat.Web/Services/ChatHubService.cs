using AspireChat.Common.Chats;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;

namespace AspireChat.Web.Services;

public interface IChatHubService : IAsyncDisposable
{
    Task ConnectAsync(int groupId, Func<GetAll.Dto, Task> onMessage, CancellationToken cancellationToken = default);
    Task DisconnectAsync(int groupId, CancellationToken cancellationToken = default);
}

public class ChatHubService(AuthenticationStateProvider authProvider, ILogger<ChatHubService> logger, IHttpMessageHandlerFactory httpMessageHandlerFactory) : IChatHubService
{
    private readonly AuthProvider _authProvider = (AuthProvider)authProvider;
    private HubConnection? _connection;
    private int? _joinedGroupId;

    public async Task ConnectAsync(int groupId, Func<GetAll.Dto, Task> onMessage, CancellationToken cancellationToken = default)
    {
        if (_connection is not null && _joinedGroupId == groupId && _connection.State == HubConnectionState.Connected)
        {
            return;
        }

        // Use Aspire service discovery base address for the API
        var baseAddress = new Uri("https://api");
        var hubUrl = new Uri(baseAddress, "/hubs/groupchat");

        // Prepare access token from AuthProvider (JWT stored in session)
        var authHeader = await _authProvider.AuthorizationHeaderValue();
        var token = authHeader.Parameter ?? string.Empty;

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl.ToString(), options =>
            {
                // Ensure negotiate and HTTP-based transports go through Aspire Service Discovery
                options.HttpMessageHandlerFactory = _ => httpMessageHandlerFactory.CreateHandler(string.Empty);

                if (!string.IsNullOrEmpty(token))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                }
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<GetAll.Dto>("ReceiveMessage", async (dto) =>
        {
            try
            {
                await onMessage(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling received message");
            }
        });

        await _connection.StartAsync(cancellationToken);
        await _connection.InvokeAsync("JoinGroup", groupId.ToString(), cancellationToken);
        _joinedGroupId = groupId;
    }

    public async Task DisconnectAsync(int groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_connection is { State: HubConnectionState.Connected })
            {
                await _connection.InvokeAsync("LeaveGroup", groupId.ToString(), cancellationToken);
                await _connection.StopAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during hub disconnect");
        }
        finally
        {
            _joinedGroupId = null;
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            try
            {
                await _connection.DisposeAsync();
            }
            catch
            {
                // ignore
            }
        }
    }
}
