using AspireChat.Common.Groups;
using AspireChat.Web.Services;

namespace AspireChat.Web.Clients;

public class GroupClient(AuthenticationService authService, HttpClient httpClient, ILogger<GroupClient> logger)
{
    public async Task<bool> CreateGroupAsync(Create.Request request, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue;
            var response = await httpClient.PostAsJsonAsync("/groups/create", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Create.Response>(cancellationToken);
            return content?.Success ?? false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Create group failed");
            return false;
        }
    }

    public async Task<IEnumerable<GetAll.Dto>> GetAllGroupsAsync(CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = authService.AuthorizationHeaderValue;
            var response = await httpClient.GetAsync("/groups", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<GetAll.Response>(cancellationToken);
            return content?.Groups ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Get all groups failed");
            return [];
        }
    }
}