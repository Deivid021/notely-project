using Bogus;
using Notely.Api.DTOs.Auth;
using Notely.Api.Models;

namespace Notely.Tests.Common.Factories;

public static class UserFactory
{
    private static readonly Faker Faker = new("pt_BR");

    public static User Create(string? email = null) => new()
    {
        Id = Guid.NewGuid(),
        Email = email ?? Faker.Internet.Email(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ValidPass123!"),
        CreatedAt = DateTime.UtcNow
    };

    public static RegisterRequest CreateRegisterRequest(string? email = null) =>
        new(email ?? Faker.Internet.Email(), "ValidPass123!");

    public static LoginRequest CreateLoginRequest(string email) =>
        new(email, "ValidPass123!");
}
