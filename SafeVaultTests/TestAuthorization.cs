using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SafeVaultTests;

public class TestAuthorization(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task AuthenticateAsync(string username, string password)
    {
        var loginContent = new FormUrlEncodedContent(
        [
        new KeyValuePair<string, string>("username", username),
        new KeyValuePair<string, string>("password", password)
        ]);

        var loginResponse = await _client.PostAsync("/login", loginContent);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Ensure the authentication cookie is set
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        Assert.NotNull(cookies);

        // Add the authentication cookie to the client
        _client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies));
    }

    private async Task AuthenticateAsUserAsync()
    {
        await AuthenticateAsync("user", "User123!");
    }

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin", "Admin123!");
    }

    [Fact]
    public async Task PublicEndpoint_ShouldBeAccessibleWithoutAuthentication()
    {
        var response = await _client.GetAsync("/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_Unauthenticated_ShouldNotBeAccessible()
    {
        var response = await _client.GetAsync("/authenticated");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_AuthenticatedUser_ShouldBeAccessible()
    {
        await AuthenticateAsUserAsync();

        var response = await _client.GetAsync("/authenticated");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UserEndpoint_Unauthenticated_ShouldNotBeAccessible()
    {
        var response = await _client.GetAsync("/user");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UserEndpoint_AuthenticatedUser_ShouldBeAccessible()
    {
        await AuthenticateAsUserAsync();

        var response = await _client.GetAsync("/user");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UserEndpoint_AuthenticatedAdmin_ShouldNotBeAccessible()
    {
        await AuthenticateAsAdminAsync();

        var response = await _client.GetAsync("/user");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_Unauthenticated_ShouldNotBeAccessible()
    {
        var response = await _client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_AuthenticatedUser_ShouldNotBeAccessible()
    {
        await AuthenticateAsUserAsync();

        var response = await _client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_AuthenticatedAdmin_ShouldBeAccessible()
    {
        await AuthenticateAsAdminAsync();

        var response = await _client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}