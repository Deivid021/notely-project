using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Notely.Api.Data;
using Notely.Api.Models;
using Notely.Api.Services;
using Notely.Tests.Common.Factories;

namespace Notely.Tests.Services;

public class AuthServiceTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static IConfiguration CreateConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "testing-secret-key-must-be-at-least-32-chars!!",
                ["Jwt:Issuer"] = "notely-api",
                ["Jwt:Audience"] = "notely-client",
                ["Jwt:ExpiresHours"] = "1",
                ["Jwt:RefreshTokenExpiryDays"] = "7"
            })
            .Build();

    [Fact]
    public async Task Register_WithNewEmail_ReturnsToken()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var req = UserFactory.CreateRegisterRequest();

        var result = await service.RegisterAsync(req);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var req = UserFactory.CreateRegisterRequest();

        await service.RegisterAsync(req);
        var result = await service.RegisterAsync(req);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Register_StoresPasswordAsHash()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var req = UserFactory.CreateRegisterRequest();

        await service.RegisterAsync(req);

        var user = await db.Users.FirstAsync();
        user.PasswordHash.Should().NotBe(req.Password);
        BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registerReq = UserFactory.CreateRegisterRequest();
        await service.RegisterAsync(registerReq);

        var result = await service.LoginAsync(UserFactory.CreateLoginRequest(registerReq.Email));

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registerReq = UserFactory.CreateRegisterRequest();
        await service.RegisterAsync(registerReq);

        var result = await service.LoginAsync(new(registerReq.Email, "WrongPassword!"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());

        var result = await service.LoginAsync(new("naoexiste@email.com", "Password123!"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsNewPairAndRevokesOld()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registered = (await service.RegisterAsync(UserFactory.CreateRegisterRequest()))!;

        var result = await service.RefreshAsync(registered.RefreshToken);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(registered.RefreshToken);

        var oldHash = HashToken(registered.RefreshToken);
        var old = await db.RefreshTokens.FirstAsync(rt => rt.TokenHash == oldHash);
        old.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_StoresHashNotRawToken()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registered = (await service.RegisterAsync(UserFactory.CreateRegisterRequest()))!;

        var raw = registered.RefreshToken;
        var stored = await db.RefreshTokens.AsNoTracking().FirstAsync();

        stored.TokenHash.Should().NotBe(raw);
        stored.TokenHash.Should().Be(HashToken(raw));
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registered = (await service.RegisterAsync(UserFactory.CreateRegisterRequest()))!;

        var stored = await db.RefreshTokens.FirstAsync();
        stored.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var result = await service.RefreshAsync(registered.RefreshToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_WithRevokedToken_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registered = (await service.RegisterAsync(UserFactory.CreateRegisterRequest()))!;

        await service.RevokeAsync(registered.RefreshToken);

        var result = await service.RefreshAsync(registered.RefreshToken);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_WithUnknownToken_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());

        var result = await service.RefreshAsync("not-a-real-token");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshAsync_TwiceWithSameToken_SecondCallReturnsNull()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registered = (await service.RegisterAsync(UserFactory.CreateRegisterRequest()))!;

        var first = await service.RefreshAsync(registered.RefreshToken);
        var second = await service.RefreshAsync(registered.RefreshToken);

        first.Should().NotBeNull();
        second.Should().BeNull();
    }

    [Fact]
    public async Task RevokeAsync_MarksTokenAsRevoked()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());
        var registered = (await service.RegisterAsync(UserFactory.CreateRegisterRequest()))!;

        await service.RevokeAsync(registered.RefreshToken);

        var stored = await db.RefreshTokens.FirstAsync();
        stored.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAsync_WithUnknownToken_DoesNotThrow()
    {
        using var db = CreateDb();
        var service = new AuthService(db, CreateConfig());

        var act = async () => await service.RevokeAsync("does-not-exist");

        await act.Should().NotThrowAsync();
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
