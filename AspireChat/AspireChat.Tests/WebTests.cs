using Microsoft.Playwright;

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

    [Fact]
    public async Task RegistrationForm_SubmitsSuccessfully()
    {
        using var webClient = fixture.CreateWebClient();
        var webUrl = webClient.BaseAddress!.ToString().TrimEnd('/');
        var uniqueEmail = $"{fixture.RunPrefix}-pwreg@example.com";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{webUrl}/Login");

        // Switch from Login to Register mode (the secondary "Register" button)
        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        // Fill registration fields (labels become visible in register mode)
        await page.GetByLabel("Email").FillAsync(uniqueEmail);
        await page.GetByLabel("Name").FillAsync("Playwright Test");
        await page.GetByLabel("Password", new() { Exact = true }).FillAsync("P@ssw0rd123!");
        await page.GetByLabel("Confirm Password").FillAsync("P@ssw0rd123!");

        // Submit the registration form
        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        // Verify we navigated away from the login page (successful registration + auto-login)
        await page.WaitForURLAsync(
            url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase),
            new() { Timeout = 15000 });

        // Ensure no error alert is shown
        Assert.False(await page.GetByRole(AriaRole.Alert).IsVisibleAsync());
    }
}
