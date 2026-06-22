using Microsoft.EntityFrameworkCore;
using Notely.Api.Data;
using Notely.Api.DTOs.Notes;
using Notely.Api.Models;

namespace Notely.Api.Services;

public class NoteService(AppDbContext db) : INoteService
{
    public async Task<IEnumerable<NoteResponse>> GetAllAsync(Guid userId, Guid? groupId = null) =>
        await db.Notes
            .Where(n => n.UserId == userId && (groupId == null || n.NoteGroupId == groupId))
            .OrderByDescending(n => n.UpdatedAt)
            .Select(n => ToResponse(n))
            .ToListAsync();

    public async Task<NoteResponse?> GetByIdAsync(Guid userId, Guid noteId) =>
        await db.Notes
            .Where(n => n.Id == noteId && n.UserId == userId)
            .Select(n => ToResponse(n))
            .FirstOrDefaultAsync();

    public async Task<NoteResponse?> CreateAsync(Guid userId, CreateNoteRequest req)
    {
        if (req.NoteGroupId.HasValue)
        {
            var groupExists = await db.NoteGroups.AnyAsync(g => g.Id == req.NoteGroupId && g.UserId == userId);
            if (!groupExists) return null;
        }

        var now = DateTime.UtcNow;
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NoteGroupId = req.NoteGroupId,
            Title = req.Title,
            Content = req.Content,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();
        return ToResponse(note);
    }

    public async Task<NoteResponse?> UpdateAsync(Guid userId, Guid noteId, UpdateNoteRequest req)
    {
        if (req.NoteGroupId.HasValue)
        {
            var groupExists = await db.NoteGroups.AnyAsync(g => g.Id == req.NoteGroupId && g.UserId == userId);
            if (!groupExists) return null;
        }

        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
        if (note is null) return null;

        note.Title = req.Title;
        note.Content = req.Content;
        note.NoteGroupId = req.NoteGroupId;
        note.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return ToResponse(note);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid noteId)
    {
        var note = await db.Notes.FirstOrDefaultAsync(n => n.Id == noteId && n.UserId == userId);
        if (note is null) return false;

        db.Notes.Remove(note);
        await db.SaveChangesAsync();
        return true;
    }

    private static NoteResponse ToResponse(Note n) =>
        new(n.Id, n.Title, n.Content, n.NoteGroupId, n.CreatedAt, n.UpdatedAt);
}
