using Notely.Api.DTOs.Notes;

namespace Notely.Api.Services;

public interface INoteService
{
    Task<IEnumerable<NoteResponse>> GetAllAsync(Guid userId, Guid? groupId = null);
    Task<NoteResponse?> GetByIdAsync(Guid userId, Guid noteId);
    Task<NoteResponse?> CreateAsync(Guid userId, CreateNoteRequest req);
    Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest req);
    Task<bool> DeleteAsync(Guid userId, Guid noteId);
}
