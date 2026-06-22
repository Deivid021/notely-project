using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Notely.Api.DTOs.Notes;
using Notely.Tests.Common.Factories;
using Notely.Tests.Common.Fixtures;

namespace Notely.Tests.Controllers;

public class NotesControllerTests : IClassFixture<IntegrationWebAppFactory>, IAsyncLifetime
{
    private readonly IntegrationWebAppFactory _factory;
    private HttpClient _client = null!;
    private Guid _userId;
    private string _token = null!;

    public NotesControllerTests(IntegrationWebAppFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _factory.ResetMocks();
        _userId = Guid.NewGuid();
        _token = AuthHelper.CreateToken(_userId);
        _client = _factory.CreateClient();
        _client.WithJwt(_token);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static NoteResponse SampleNote(Guid? id = null) => new(
        id ?? Guid.NewGuid(),
        "Title",
        "Content",
        null,
        DateTime.UtcNow,
        DateTime.UtcNow
    );

    [Fact]
    public async Task GetAll_Authenticated_Returns200WithList()
    {
        var notes = new[] { SampleNote(), SampleNote() };
        _factory.NoteService.GetAllAsync(_userId, null).Returns(notes);

        var response = await _client.GetAsync("/notes");
        var body = await response.Content.ReadFromJsonAsync<List<NoteResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithGroupId_PassesFilterToService()
    {
        var groupId = Guid.NewGuid();
        _factory.NoteService.GetAllAsync(_userId, groupId).Returns(Array.Empty<NoteResponse>());

        var response = await _client.GetAsync($"/notes?groupId={groupId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await _factory.NoteService.Received(1).GetAllAsync(_userId, groupId);
    }

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/notes");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Existing_Returns200()
    {
        var note = SampleNote();
        _factory.NoteService.GetByIdAsync(_userId, note.Id).Returns(note);

        var response = await _client.GetAsync($"/notes/{note.Id}");
        var body = await response.Content.ReadFromJsonAsync<NoteResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Id.Should().Be(note.Id);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        _factory.NoteService.GetByIdAsync(_userId, Arg.Any<Guid>()).ReturnsNull();

        var response = await _client.GetAsync($"/notes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocation()
    {
        var req = NoteFactory.CreateRequest();
        var created = SampleNote() with { Title = req.Title, Content = req.Content };
        _factory.NoteService.CreateAsync(_userId, Arg.Any<CreateNoteRequest>()).Returns(created);

        var response = await _client.PostAsJsonAsync("/notes", req);
        var body = await response.Content.ReadFromJsonAsync<NoteResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        body!.Title.Should().Be(req.Title);
    }

    [Fact]
    public async Task Create_GroupNotOwned_Returns422()
    {
        _factory.NoteService.CreateAsync(_userId, Arg.Any<CreateNoteRequest>()).ReturnsNull();

        var response = await _client.PostAsJsonAsync("/notes",
            NoteFactory.CreateRequest(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Update_Existing_Returns200()
    {
        var noteId = Guid.NewGuid();
        var req = NoteFactory.UpdateRequest();
        var updated = SampleNote(noteId) with { Title = req.Title, Content = req.Content };
        _factory.NoteService.UpdateAsync(_userId, noteId, Arg.Any<UpdateNoteRequest>()).Returns(updated);

        var response = await _client.PutAsJsonAsync($"/notes/{noteId}", req);
        var body = await response.Content.ReadFromJsonAsync<NoteResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Title.Should().Be(req.Title);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        _factory.NoteService.UpdateAsync(_userId, Arg.Any<Guid>(), Arg.Any<UpdateNoteRequest>()).ReturnsNull();

        var response = await _client.PutAsJsonAsync($"/notes/{Guid.NewGuid()}", NoteFactory.UpdateRequest());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Existing_Returns204()
    {
        _factory.NoteService.DeleteAsync(_userId, Arg.Any<Guid>()).Returns(true);

        var response = await _client.DeleteAsync($"/notes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        _factory.NoteService.DeleteAsync(_userId, Arg.Any<Guid>()).Returns(false);

        var response = await _client.DeleteAsync($"/notes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
