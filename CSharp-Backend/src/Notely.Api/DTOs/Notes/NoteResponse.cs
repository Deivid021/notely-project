namespace Notely.Api.DTOs.Notes;

public record NoteResponse(
    Guid Id,
    string Title,
    string Content,
    Guid? NoteGroupId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
