namespace Notely.Api.DTOs.NoteGroups;

public record NoteGroupResponse(
    Guid Id,
    string Name,
    string? Description,
    int NoteCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
