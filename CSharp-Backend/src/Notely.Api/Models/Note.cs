namespace Notely.Api.Models;

public class Note
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? NoteGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public NoteGroup? NoteGroup { get; set; }
}
