using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Notely.Api.DTOs.NoteGroups;
using Notely.Tests.Common.Factories;
using Notely.Tests.Common.Fixtures;

namespace Notely.Tests.Controllers;

public class NoteGroupsControllerTests : IClassFixture<IntegrationWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationWebAppFactory _factory;
    private HttpClient _client = null!;
    private Guid _userId;

    public NoteGroupsControllerTests(IntegrationWebAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _factory.ResetMocks();
        _userId = Guid.NewGuid();
        _client = _factory.CreateClient();
        _client.WithJwt(AuthHelper.CreateToken(_userId));
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static NoteGroupResponse SampleGroup(Guid? id = null, string name = "Trabalho") => new(
        id ?? Guid.NewGuid(),
        name,
        "desc",
        0,
        DateTime.UtcNow,
        DateTime.UtcNow
    );

    [Fact]
    public async Task GetAll_Authenticated_Returns200WithList()
    {
        _factory.NoteGroupService.GetAllAsync(_userId).Returns(new[] { SampleGroup(), SampleGroup() });

        var response = await _client.GetAsync("/note-groups");
        var body = await response.Content.ReadFromJsonAsync<List<NoteGroupResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/note-groups");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var group = SampleGroup();
        _factory.NoteGroupService.GetByIdAsync(_userId, group.Id).Returns(group);

        var response = await _client.GetAsync($"/note-groups/{group.Id}");
        var body = await response.Content.ReadFromJsonAsync<NoteGroupResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(group.Id);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _factory.NoteGroupService.GetByIdAsync(_userId, Arg.Any<Guid>()).ReturnsNull();

        var response = await _client.GetAsync($"/note-groups/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocation()
    {
        var req = NoteGroupFactory.CreateRequest();
        var created = SampleGroup(name: req.Name);
        _factory.NoteGroupService.CreateAsync(_userId, Arg.Any<CreateNoteGroupRequest>()).Returns(created);

        var response = await _client.PostAsJsonAsync("/note-groups", req);
        var body = await response.Content.ReadFromJsonAsync<NoteGroupResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        body!.Name.Should().Be(req.Name);
    }

    [Fact]
    public async Task Update_Existing_Returns200()
    {
        var groupId = Guid.NewGuid();
        var req = NoteGroupFactory.UpdateRequest();
        var updated = SampleGroup(groupId, req.Name);
        _factory.NoteGroupService.UpdateAsync(_userId, groupId, Arg.Any<UpdateNoteGroupRequest>()).Returns(updated);

        var response = await _client.PutAsJsonAsync($"/note-groups/{groupId}", req);
        var body = await response.Content.ReadFromJsonAsync<NoteGroupResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Name.Should().Be(req.Name);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _factory.NoteGroupService.UpdateAsync(_userId, Arg.Any<Guid>(), Arg.Any<UpdateNoteGroupRequest>())
            .ReturnsNull();

        var response = await _client.PutAsJsonAsync($"/note-groups/{Guid.NewGuid()}",
            NoteGroupFactory.UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        _factory.NoteGroupService.DeleteAsync(_userId, Arg.Any<Guid>()).Returns(true);

        var response = await _client.DeleteAsync($"/note-groups/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _factory.NoteGroupService.DeleteAsync(_userId, Arg.Any<Guid>()).Returns(false);

        var response = await _client.DeleteAsync($"/note-groups/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
