using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notely.Api.DTOs.NoteGroups;
using Notely.Api.Services;

namespace Notely.Api.Controllers;

[ApiController]
[Route("note-groups")]
[Authorize]
public class NoteGroupsController(INoteGroupService noteGroupService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await noteGroupService.GetAllAsync(UserId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var group = await noteGroupService.GetByIdAsync(UserId, id);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateNoteGroupRequest req)
    {
        var group = await noteGroupService.CreateAsync(UserId, req);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateNoteGroupRequest req)
    {
        var group = await noteGroupService.UpdateAsync(UserId, id, req);
        return group is null ? NotFound() : Ok(group);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await noteGroupService.DeleteAsync(UserId, id);
        return deleted ? NoContent() : NotFound();
    }
}
