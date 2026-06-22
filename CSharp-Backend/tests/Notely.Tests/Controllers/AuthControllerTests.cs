using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Notely.Api.DTOs.Auth;
using Notely.Tests.Common.Factories;
using Notely.Tests.Common.Fixtures;

namespace Notely.Tests.Controllers;

public class AuthControllerTests : IClassFixture<IntegrationWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationWebAppFactory _factory;
    private HttpClient _client = null!;

    public AuthControllerTests(IntegrationWebAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _factory.ResetMocks();
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ValidRequest_Returns200WithToken()
    {
        var req = UserFactory.CreateRegisterRequest();
        var expected = new AuthResponse("access-token", "refresh-token");
        _factory.AuthService.RegisterAsync(Arg.Any<RegisterRequest>()).Returns(expected);

        var response = await _client.PostAsJsonAsync("/auth/register", req);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().Be("access-token");
        body.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        _factory.AuthService.RegisterAsync(Arg.Any<RegisterRequest>()).ReturnsNull();

        var response = await _client.PostAsJsonAsync("/auth/register",
            UserFactory.CreateRegisterRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_InvalidBody_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/auth/register",
            new RegisterRequest("not-an-email", "short"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await _factory.AuthService.DidNotReceive().RegisterAsync(Arg.Any<RegisterRequest>());
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var expected = new AuthResponse("access-token", "refresh-token");
        _factory.AuthService.LoginAsync(Arg.Any<LoginRequest>()).Returns(expected);

        var response = await _client.PostAsJsonAsync("/auth/login",
            UserFactory.CreateLoginRequest("user@example.com"));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        _factory.AuthService.LoginAsync(Arg.Any<LoginRequest>()).ReturnsNull();

        var response = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest("user@example.com", "WrongPassword123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_Returns200WithNewPair()
    {
        var expected = new AuthResponse("new-access", "new-refresh");
        _factory.AuthService.RefreshAsync("old-refresh").Returns(expected);

        var response = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshRequest("old-refresh"));
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.AccessToken.Should().Be("new-access");
        body.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        _factory.AuthService.RefreshAsync(Arg.Any<string>()).ReturnsNull();

        var response = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshRequest("bogus"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent_AndCallsRevoke()
    {
        var response = await _client.PostAsJsonAsync("/auth/logout",
            new RefreshRequest("any-token"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await _factory.AuthService.Received(1).RevokeAsync("any-token");
    }
}
