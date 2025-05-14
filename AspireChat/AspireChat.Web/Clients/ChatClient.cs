using AspireChat.Common.Chats;
using AspireChat.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace AspireChat.Web.Clients;

public class ChatClient(
    AuthenticationStateProvider authProvider,
    HttpClient httpClient,
    ILogger<ChatClient> logger)
{
    private AuthProvider AuthProvider => (AuthProvider)authProvider;

    public async Task<IEnumerable<GetAll.Dto>> GetAllChatsAsync(int groupId, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = await AuthProvider.AuthorizationHeaderValue();
            var response = await httpClient.GetAsync($"/chats/{groupId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<GetAll.Response>(cancellationToken);
            return content?.Chats ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Get all chats failed");
            return [];
        }
    }

    public async Task SendMessageAsync(int groupId, string message, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = await AuthProvider.AuthorizationHeaderValue();
            var req = new Send.Request { GroupId = groupId, Message = message };
            var response = await httpClient.PostAsJsonAsync($"/chats/{groupId}", req, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Send message failed");
        }
    }
}