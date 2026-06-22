using System.ComponentModel.DataAnnotations;

namespace Notely.Api.DTOs.Notes;

public record CreateNoteRequest(
    [Required, MaxLength(255)] string Title,
    [Required] string Content,
    Guid? NoteGroupId
);
