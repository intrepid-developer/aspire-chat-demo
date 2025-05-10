using AspireChat.Common.Chats;
using AspireChat.Web.Services;

namespace AspireChat.Web.Clients;

public class ChatClient(AuthenticationService authService, HttpClient httpClient, ILogger<ChatClient> logger)
{
    public async Task<IEnumerable<GetAll.Dto>> GetAllChatsAsync(int groupId, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue();
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
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue();
            var req = new Send.Request(groupId, message);
            var response = await httpClient.PostAsJsonAsync($"/chats/{groupId}", req, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Send message failed");
        }
    }
}
