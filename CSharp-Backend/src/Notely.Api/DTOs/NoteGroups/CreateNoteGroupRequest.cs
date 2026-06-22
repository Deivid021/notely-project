using System.ComponentModel.DataAnnotations;

namespace Notely.Api.DTOs.NoteGroups;

public record CreateNoteGroupRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description
);
