using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERBMS.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(dto, cancellationToken);
        return user is null ? BadRequest("User creation failed.") : CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var updated = await _userService.UpdateAsync(id, dto, cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var currentUserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(currentUserIdValue, out var currentUserId) && currentUserId == id)
        {
            return BadRequest("You cannot delete your own account.");
        }

        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (user.IsSystemAdmin)
        {
            return BadRequest("System admin cannot be deleted.");
        }

        var deleted = await _userService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto, CancellationToken cancellationToken)
    {
        var assigned = await _userService.AssignRoleAsync(dto, cancellationToken);
        return assigned ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var approved = await _userService.ApproveAsync(id, cancellationToken);
        return approved ? Ok() : NotFound();
    }

    [HttpPost("remove-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto, CancellationToken cancellationToken)
    {
        var removed = await _userService.RemoveRoleAsync(dto, cancellationToken);
        return removed ? Ok() : NotFound();
    }

    [HttpPost("cleanup-identity")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupIdentity([FromBody] CleanupIdentityUserDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email is required.");
        }

        var result = await _userService.CleanupIdentityUserAsync(dto, cancellationToken);
        return result switch
        {
            CleanupIdentityResult.Deleted => Ok(),
            CleanupIdentityResult.InUse => BadRequest("User exists in the app; delete from users list instead."),
            CleanupIdentityResult.Forbidden => BadRequest("System admin cannot be deleted."),
            _ => NotFound()
        };
    }
}
