using Microsoft.EntityFrameworkCore;
using Notely.Api.Data;
using Notely.Api.DTOs.NoteGroups;
using Notely.Api.Models;

namespace Notely.Api.Services;

public class NoteGroupService(AppDbContext db) : INoteGroupService
{
    public async Task<IEnumerable<NoteGroupResponse>> GetAllAsync(Guid userId) =>
        await db.NoteGroups
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.Name)
            .Select(g => new NoteGroupResponse(g.Id, g.Name, g.Description, g.Notes.Count, g.CreatedAt, g.UpdatedAt))
            .ToListAsync();

    public async Task<NoteGroupResponse?> GetByIdAsync(Guid userId, Guid groupId) =>
        await db.NoteGroups
            .Where(g => g.Id == groupId && g.UserId == userId)
            .Select(g => new NoteGroupResponse(g.Id, g.Name, g.Description, g.Notes.Count, g.CreatedAt, g.UpdatedAt))
            .FirstOrDefaultAsync();

    public async Task<NoteGroupResponse> CreateAsync(Guid userId, CreateNoteGroupRequest req)
    {
        var now = DateTime.UtcNow;
        var group = new NoteGroup
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = req.Name,
            Description = req.Description,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.NoteGroups.Add(group);
        await db.SaveChangesAsync();
        return new NoteGroupResponse(group.Id, group.Name, group.Description, 0, group.CreatedAt, group.UpdatedAt);
    }

    public async Task<NoteGroupResponse?> UpdateAsync(Guid userId, Guid groupId, UpdateNoteGroupRequest req)
    {
        var group = await db.NoteGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);
        if (group is null) return null;

        group.Name = req.Name;
        group.Description = req.Description;
        group.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return await GetByIdAsync(userId, groupId);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid groupId)
    {
        var group = await db.NoteGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);
        if (group is null) return false;

        db.NoteGroups.Remove(group);
        await db.SaveChangesAsync();
        return true;
    }
}
