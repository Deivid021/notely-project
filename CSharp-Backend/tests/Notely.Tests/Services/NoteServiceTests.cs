using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Notely.Api.Data;
using Notely.Api.Services;
using Notely.Tests.Common.Factories;

namespace Notely.Tests.Services;

public class NoteServiceTests
{
    private static AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<Guid> SeedUserAsync(AppDbContext db)
    {
        var user = UserFactory.Create();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyNotesOfUser()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        db.Notes.AddRange(NoteFactory.Create(userId1), NoteFactory.Create(userId1), NoteFactory.Create(userId2));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync(userId1);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(n => db.Notes.Any(dbNote => dbNote.Id == n.Id && dbNote.UserId == userId1));
    }

    [Fact]
    public async Task GetAll_EmptyUser_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId = await SeedUserAsync(db);

        var result = await service.GetAllAsync(userId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_ExistingNote_ReturnsNote()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId = await SeedUserAsync(db);
        var note = NoteFactory.Create(userId);
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var result = await service.GetByIdAsync(userId, note.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(note.Id);
        result.Title.Should().Be(note.Title);
    }

    [Fact]
    public async Task GetById_NoteOfOtherUser_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        var note = NoteFactory.Create(userId1);
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var result = await service.GetByIdAsync(userId2, note.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId = await SeedUserAsync(db);

        var result = await service.GetByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Create_ValidRequest_PersistsAndReturns()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId = await SeedUserAsync(db);
        var req = NoteFactory.CreateRequest();

        var result = await service.CreateAsync(userId, req);

        result.Should().NotBeNull();
        result.Title.Should().Be(req.Title);
        result.Content.Should().Be(req.Content);
        db.Notes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Update_ExistingNote_ReturnsUpdated()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId = await SeedUserAsync(db);
        var note = NoteFactory.Create(userId);
        db.Notes.Add(note);
        await db.SaveChangesAsync();
        var req = NoteFactory.UpdateRequest();

        var result = await service.UpdateAsync(userId, note.Id, req);

        result.Should().NotBeNull();
        result!.Title.Should().Be(req.Title);
        result.Content.Should().Be(req.Content);
    }

    [Fact]
    public async Task Update_NoteOfOtherUser_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        var note = NoteFactory.Create(userId1);
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(userId2, note.Id, NoteFactory.UpdateRequest());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_OwnNote_ReturnsTrueAndRemoves()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId = await SeedUserAsync(db);
        var note = NoteFactory.Create(userId);
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var result = await service.DeleteAsync(userId, note.Id);

        result.Should().BeTrue();
        db.Notes.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_NoteOfOtherUser_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new NoteService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        var note = NoteFactory.Create(userId1);
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var result = await service.DeleteAsync(userId2, note.Id);

        result.Should().BeFalse();
        db.Notes.Should().HaveCount(1);
    }
}
