using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Notely.Api.Data;
using Notely.Api.Services;
using Notely.Tests.Common.Factories;

namespace Notely.Tests.Services;

public class NoteGroupServiceTests
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
    public async Task GetAll_ReturnsOnlyGroupsOfUser()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        db.NoteGroups.AddRange(NoteGroupFactory.Create(userId1), NoteGroupFactory.Create(userId2));
        await db.SaveChangesAsync();

        var result = await service.GetAllAsync(userId1);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ExistingGroup_ReturnsGroup()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId = await SeedUserAsync(db);
        var group = NoteGroupFactory.Create(userId);
        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();

        var result = await service.GetByIdAsync(userId, group.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(group.Id);
        result.Name.Should().Be(group.Name);
    }

    [Fact]
    public async Task GetById_GroupOfOtherUser_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        var group = NoteGroupFactory.Create(userId1);
        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();

        var result = await service.GetByIdAsync(userId2, group.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Create_ValidRequest_PersistsAndReturns()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId = await SeedUserAsync(db);
        var req = NoteGroupFactory.CreateRequest();

        var result = await service.CreateAsync(userId, req);

        result.Should().NotBeNull();
        result.Name.Should().Be(req.Name);
        db.NoteGroups.Should().HaveCount(1);
    }

    [Fact]
    public async Task Update_ExistingGroup_ReturnsUpdated()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId = await SeedUserAsync(db);
        var group = NoteGroupFactory.Create(userId);
        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();
        var req = NoteGroupFactory.UpdateRequest();

        var result = await service.UpdateAsync(userId, group.Id, req);

        result.Should().NotBeNull();
        result!.Name.Should().Be(req.Name);
    }

    [Fact]
    public async Task Update_GroupOfOtherUser_ReturnsNull()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        var group = NoteGroupFactory.Create(userId1);
        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();

        var result = await service.UpdateAsync(userId2, group.Id, NoteGroupFactory.UpdateRequest());

        result.Should().BeNull();
    }

    [Fact]
    public async Task Delete_OwnGroup_ReturnsTrueAndRemoves()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId = await SeedUserAsync(db);
        var group = NoteGroupFactory.Create(userId);
        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();

        var result = await service.DeleteAsync(userId, group.Id);

        result.Should().BeTrue();
        db.NoteGroups.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_GroupOfOtherUser_ReturnsFalse()
    {
        using var db = CreateDb();
        var service = new NoteGroupService(db);
        var userId1 = await SeedUserAsync(db);
        var userId2 = await SeedUserAsync(db);
        var group = NoteGroupFactory.Create(userId1);
        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();

        var result = await service.DeleteAsync(userId2, group.Id);

        result.Should().BeFalse();
        db.NoteGroups.Should().HaveCount(1);
    }
}
