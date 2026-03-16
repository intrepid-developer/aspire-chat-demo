namespace AspireChat.Tests;

[Collection(DistributedApplicationCollection.Name)]
public sealed class WebTests(DistributedApplicationFixture fixture)
{
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        using var httpClient = fixture.CreateWebClient();
        using var response = await httpClient.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
