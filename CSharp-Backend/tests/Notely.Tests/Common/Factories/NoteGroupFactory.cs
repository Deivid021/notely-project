using Bogus;
using Notely.Api.DTOs.NoteGroups;
using Notely.Api.Models;

namespace Notely.Tests.Common.Factories;

public static class NoteGroupFactory
{
    private static readonly Faker Faker = new("pt_BR");

    public static NoteGroup Create(Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = Faker.Commerce.Department(),
        Description = Faker.Lorem.Sentence(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static CreateNoteGroupRequest CreateRequest() =>
        new(Faker.Commerce.Department(), Faker.Lorem.Sentence());

    public static UpdateNoteGroupRequest UpdateRequest() =>
        new(Faker.Commerce.Department(), Faker.Lorem.Sentence());
}
