using Bogus;
using Notely.Api.DTOs.Notes;
using Notely.Api.Models;

namespace Notely.Tests.Common.Factories;

public static class NoteFactory
{
    private static readonly Faker Faker = new("pt_BR");

    public static Note Create(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = Faker.Lorem.Sentence(3),
        Content = Faker.Lorem.Paragraph(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static CreateNoteRequest CreateRequest(Guid? noteGroupId = null) =>
        new(Faker.Lorem.Sentence(3), Faker.Lorem.Paragraph(), noteGroupId);

    public static UpdateNoteRequest UpdateRequest(Guid? noteGroupId = null) =>
        new(Faker.Lorem.Sentence(3), Faker.Lorem.Paragraph(), noteGroupId);
}
