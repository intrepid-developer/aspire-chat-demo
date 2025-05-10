using AspireChat.Common.Users;
using AspireChat.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace AspireChat.Web.Clients;

public class UserClient(
    AuthenticationStateProvider authProvider,
    HttpClient httpClient,
    ILogger<UserClient> logger)
{
    private AuthProvider AuthProvider => (AuthProvider)authProvider;
    public async Task<bool> LoginAsync(Login.Request request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/users/login", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Login.Response>(cancellationToken);
            
            var token = content?.Token ?? string.Empty;

            await AuthProvider.AuthenticateUser(token);

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Login failed");
            return false;
        }
    }

    public async Task<bool> RegisterAsync(Register.Request request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/users/register", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Register.Response>(cancellationToken);
            
            var token = content?.Token ?? string.Empty;

            await AuthProvider.AuthenticateUser(token);

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Register failed");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Update.Request request, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = await AuthProvider.AuthorizationHeaderValue();
            var response = await httpClient.PutAsJsonAsync("/users", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<Update.Response>(cancellationToken);
            return content?.Success ?? false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Update failed");
            return false;
        }
    }

    public async Task<GetProfile.Response?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = await AuthProvider.AuthorizationHeaderValue();
            var response = await httpClient.GetAsync("/users/profile", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<GetProfile.Response>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "GetProfile failed");
            return null;
        }
    }

    public async Task<string?> UploadImageAsync(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = await AuthProvider.AuthorizationHeaderValue();
            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream(5 * 1024 * 1024); // 5MB limit
            content.Add(new StreamContent(stream), "Image", file.Name);
            content.Add(new StringContent(file.Name), "ImageName");
            var response = await httpClient.PostAsync("/users/upload-image", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UploadImage.Response>(cancellationToken);
            return result?.ImageUrl;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "UploadImage failed");
            return null;
        }
    }
}
