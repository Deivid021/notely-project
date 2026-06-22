using Notely.Api.DTOs.NoteGroups;

namespace Notely.Api.Services;

public interface INoteGroupService
{
    Task<IEnumerable<NoteGroupResponse>> GetAllAsync(Guid userId);
    Task<NoteGroupResponse?> GetByIdAsync(Guid userId, Guid groupId);
    Task<NoteGroupResponse> CreateAsync(Guid userId, CreateNoteGroupRequest req);
    Task<NoteGroupResponse?> UpdateAsync(Guid userId, Guid groupId, UpdateNoteGroupRequest req);
    Task<bool> DeleteAsync(Guid userId, Guid groupId);
}
