using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AspireChat.Common.Chats;
using AspireChat.Common.Groups;
using AspireChat.Common.Users;
using VerifyTests;

namespace AspireChat.Tests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class ApiTests(DistributedApplicationFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task RegisterEndpointReturnsSuccessSnapshot()
    {
        var user = await RegisterUserAsync("register", "Register User", "register-password");

        await Verifier.Verify(new
        {
            StatusCode = (int)user.StatusCode,
            user.Response.Success,
            TokenPresent = !string.IsNullOrWhiteSpace(user.Response.Token)
        });
    }

    [Fact]
    public async Task LoginEndpointReturnsSuccessSnapshot()
    {
        var registeredUser = await RegisterUserAsync("login", "Login User", "login-password");
        using var client = fixture.CreateApiClient();
        using var response = await client.PostAsJsonAsync("/users/login", new Login.Request
        {
            Email = registeredUser.Email,
            Password = registeredUser.Password
        }, TestContext.Current.CancellationToken);

        var body = await ReadJsonAsync<Login.Response>(response);

        await Verifier.Verify(new
        {
            StatusCode = (int)response.StatusCode,
            body.Success,
            TokenPresent = !string.IsNullOrWhiteSpace(body.Token)
        });
    }

    [Fact]
    public async Task GetProfileEndpointReturnsSnapshotForAuthenticatedUser()
    {
        var registeredUser = await RegisterUserAsync("profile", "Profile User", "profile-password");
        using var client = CreateAuthenticatedApiClient(registeredUser.Response.Token!);
        using var response = await client.GetAsync("/users/profile", TestContext.Current.CancellationToken);

        var profile = await ReadJsonAsync<GetProfile.Response>(response);

        await Verifier.Verify(new
        {
            StatusCode = (int)response.StatusCode,
            Profile = new
            {
                profile.Name,
                Email = "<registered-user-email>",
                profile.ProfileImageUrl
            }
        });
    }

    [Fact]
    public async Task UpdateEndpointPersistsProfileChangesSnapshot()
    {
        var registeredUser = await RegisterUserAsync("update", "Original User", "update-password");
        using var client = CreateAuthenticatedApiClient(registeredUser.Response.Token!);

        using var updateResponse = await client.PutAsJsonAsync("/users", new Update.Request(
            Name: "Updated User",
            Email: registeredUser.Email.Replace("@", ".updated@", StringComparison.Ordinal),
            ProfileImageUrl: "https://cdn.example.com/avatar.png",
            Password: "updated-password"), TestContext.Current.CancellationToken);

        var update = await ReadJsonAsync<Update.Response>(updateResponse);

        using var profileResponse = await client.GetAsync("/users/profile", TestContext.Current.CancellationToken);
        var profile = await ReadJsonAsync<GetProfile.Response>(profileResponse);

        await Verifier.Verify(new
        {
            UpdateStatusCode = (int)updateResponse.StatusCode,
            update.Success,
            ProfileStatusCode = (int)profileResponse.StatusCode,
            Profile = new
            {
                profile.Name,
                Email = "<updated-user-email>",
                profile.ProfileImageUrl
            }
        });
    }

    [Fact]
    public async Task GetGroupsEndpointReturnsSnapshotForCreatedGroups()
    {
        var registeredUser = await RegisterUserAsync("groups", "Group User", "groups-password");
        using var client = CreateAuthenticatedApiClient(registeredUser.Response.Token!);

        var createdGroups = new[]
        {
            $"{fixture.RunPrefix}-alpha",
            $"{fixture.RunPrefix}-beta"
        };
        var expectedGroups = createdGroups
            .Select(group => group.Replace($"{fixture.RunPrefix}-", string.Empty, StringComparison.Ordinal))
            .OrderBy(group => group, StringComparer.Ordinal)
            .ToArray();

        foreach (var groupName in createdGroups)
        {
            using var createResponse = await client.PostAsJsonAsync("/groups", new Create.Request
            {
                Name = groupName
            }, TestContext.Current.CancellationToken);

            var create = await ReadJsonAsync<Create.Response>(createResponse);
            Assert.True(create.Success);
        }

        using var response = await client.GetAsync("/groups", TestContext.Current.CancellationToken);
        var body = await ReadJsonAsync<AspireChat.Common.Groups.GetAll.Response>(response);

        var groups = body.Groups
            .Where(group => createdGroups.Contains(group.Name, StringComparer.Ordinal))
            .OrderBy(group => group.Name, StringComparer.Ordinal)
            .Select(group => group.Name.Replace($"{fixture.RunPrefix}-", string.Empty, StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(expectedGroups, groups);

        await Verifier.Verify(new
        {
            StatusCode = (int)response.StatusCode,
            Groups = groups
        });
    }

    [Fact]
    public async Task ChatsEndpointsReturnStableSnapshotForCreatedMessages()
    {
        var sender = await RegisterUserAsync("chat-sender", "Chat Sender", "chat-password");
        var receiver = await RegisterUserAsync("chat-receiver", "Chat Receiver", "chat-password");

        using var senderClient = CreateAuthenticatedApiClient(sender.Response.Token!);
        using var receiverClient = CreateAuthenticatedApiClient(receiver.Response.Token!);

        var groupName = $"{fixture.RunPrefix}-chat-room";
        using var createGroupResponse = await senderClient.PostAsJsonAsync("/groups", new Create.Request
        {
            Name = groupName
        }, TestContext.Current.CancellationToken);
        var createGroup = await ReadJsonAsync<Create.Response>(createGroupResponse);
        Assert.True(createGroup.Success);

        var groupId = await GetGroupIdByNameAsync(senderClient, groupName);
        var message = $"{fixture.RunPrefix}-hello from sender";

        using var sendResponse = await senderClient.PostAsJsonAsync($"/chats/{groupId}", new Send.Request
        {
            GroupId = groupId,
            Message = message
        }, TestContext.Current.CancellationToken);
        var send = await ReadJsonAsync<Send.Response>(sendResponse);

        using var senderChatsResponse = await senderClient.GetAsync($"/chats/{groupId}", TestContext.Current.CancellationToken);
        var senderChats = await ReadJsonAsync<AspireChat.Common.Chats.GetAll.Response>(senderChatsResponse);

        using var receiverChatsResponse = await receiverClient.GetAsync($"/chats/{groupId}", TestContext.Current.CancellationToken);
        var receiverChats = await ReadJsonAsync<AspireChat.Common.Chats.GetAll.Response>(receiverChatsResponse);

        var senderView = senderChats.Chats
            .Where(chat => chat.Message == message)
            .OrderBy(chat => chat.Name, StringComparer.Ordinal)
            .Select(chat => new
            {
                chat.Name,
                Message = "<chat-message>",
                chat.IsMe,
                HasAvatar = chat.UserAvatarUrl is not null
            })
            .ToArray();

        var receiverView = receiverChats.Chats
            .Where(chat => chat.Message == message)
            .OrderBy(chat => chat.Name, StringComparer.Ordinal)
            .Select(chat => new
            {
                chat.Name,
                Message = "<chat-message>",
                chat.IsMe,
                HasAvatar = chat.UserAvatarUrl is not null
            })
            .ToArray();

        await Verifier.Verify(new
        {
            CreateGroupStatusCode = (int)createGroupResponse.StatusCode,
            SendStatusCode = (int)sendResponse.StatusCode,
            send.Success,
            SenderChatStatusCode = (int)senderChatsResponse.StatusCode,
            ReceiverChatStatusCode = (int)receiverChatsResponse.StatusCode,
            SenderView = senderView,
            ReceiverView = receiverView
        });
    }

    [Fact]
    public async Task UploadImageEndpointReturnsStableSnapshot()
    {
        var registeredUser = await RegisterUserAsync("upload", "Upload User", "upload-password");
        using var client = CreateAuthenticatedApiClient(registeredUser.Response.Token!);

        using var imageContent = new ByteArrayContent("fake-image-payload"u8.ToArray());
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var form = new MultipartFormDataContent();
        form.Add(imageContent, "Image", "avatar.png");
        form.Add(new StringContent("avatar"), "ImageName");

        using var response = await client.PostAsync("/users/upload-image", form, TestContext.Current.CancellationToken);
        var body = await ReadJsonAsync<UploadImage.Response>(response);
        var imageUrl = new Uri(body.ImageUrl);

        await Verifier.Verify(new
        {
            StatusCode = (int)response.StatusCode,
            imageUrl.Scheme,
            imageUrl.Host,
            Segments = imageUrl.Segments,
            HasImagesContainer = imageUrl.AbsolutePath.Contains("/images/", StringComparison.Ordinal),
            EndsWithPng = imageUrl.AbsolutePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
        });
    }

    [Fact]
    public async Task ChatsEndpointRequiresAuthentication()
    {
        using var client = fixture.CreateApiClient();
        using var response = await client.GetAsync("/chats/9999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadImageEndpointRequiresAuthentication()
    {
        using var client = fixture.CreateApiClient();
        using var imageContent = new ByteArrayContent("fake-image-payload"u8.ToArray());
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        using var form = new MultipartFormDataContent();
        form.Add(imageContent, "Image", "avatar.png");
        form.Add(new StringContent("avatar"), "ImageName");

        using var response = await client.PostAsync("/users/upload-image", form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateAuthenticatedApiClient(string token)
    {
        var client = fixture.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<RegisteredUser> RegisterUserAsync(string scenario, string name, string password)
    {
        using var client = fixture.CreateApiClient();
        var email = $"{scenario}.{fixture.RunPrefix}@example.com";

        using var response = await client.PostAsJsonAsync("/users/register", new Register.Request
        {
            Name = name,
            Email = email,
            Password = password
        }, TestContext.Current.CancellationToken);

        var body = await ReadJsonAsync<Register.Response>(response);
        Assert.True(body.Success);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));

        return new RegisteredUser(email, password, response.StatusCode, body);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode,
            $"Expected a successful response but received {(int)response.StatusCode} {response.ReasonPhrase}.{Environment.NewLine}{content}");

        var result = JsonSerializer.Deserialize<T>(content, JsonOptions);
        Assert.NotNull(result);

        return result;
    }

    private static async Task<int> GetGroupIdByNameAsync(HttpClient client, string groupName)
    {
        using var response = await client.GetAsync("/groups", TestContext.Current.CancellationToken);
        var body = await ReadJsonAsync<AspireChat.Common.Groups.GetAll.Response>(response);
        var group = body.Groups.Single(group => group.Name == groupName);
        return group.Id;
    }

    private sealed record RegisteredUser(string Email, string Password, HttpStatusCode StatusCode, Register.Response Response);
}
