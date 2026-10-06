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
        var browserLogs = new List<string>();
        page.Console += (_, msg) => browserLogs.Add($"{msg.Type}: {msg.Text}");
        page.PageError += (_, msg) => browserLogs.Add($"pageerror: {msg}");

        // Increase default timeout for CI environments (GitHub runners can be slower)
        page.SetDefaultTimeout(60000);

        await page.GotoAsync($"{webUrl}/Login", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await page.GetByText("Don't have an account?").WaitForAsync(new() { Timeout = 30000 });
        await page.WaitForFunctionAsync("() => window.Blazor", new PageWaitForFunctionOptions { Timeout = 30000 });

        // Switch from Login to Register mode. MudBlazor 9.11 floating labels are not always
        // exposed to GetByLabel, so the form uses stable InputId values instead.
        var registerName = page.Locator("#register-name");
        var switched = false;
        for (var attempt = 0; attempt < 5 && !switched; attempt++)
        {
            await page.Locator("#toggle-auth-mode").ClickAsync();
            try
            {
                await registerName.WaitForAsync(new LocatorWaitForOptions { Timeout = 3000 });
                switched = true;
            }
            catch (TimeoutException)
            {
                await page.WaitForTimeoutAsync(500);
            }
        }

        if (!switched)
        {
            throw new TimeoutException(
                $"Register form did not appear after toggling auth mode. URL={page.Url}. Browser logs:{Environment.NewLine}{string.Join(Environment.NewLine, browserLogs)}");
        }

        await page.Locator("#register-email").FillAsync(uniqueEmail);
        await registerName.FillAsync("Playwright Test");
        await page.Locator("#register-password").FillAsync("P@ssw0rd123!");
        await page.Locator("#register-confirm-password").FillAsync("P@ssw0rd123!");

        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        // Verify we navigated away from the login page (successful registration + auto-login)
        await page.WaitForURLAsync(
            url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase),
            new() { Timeout = 30000 });

        // Ensure no error alert is shown
        Assert.False(await page.GetByRole(AriaRole.Alert).IsVisibleAsync());
    }
}
