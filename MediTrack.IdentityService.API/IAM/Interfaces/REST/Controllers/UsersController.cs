using System.Security.Claims;
using MediTrack.IdentityService.API.IAM.Domain.Model.Queries;
using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserQueryService _userQueryService;
    private readonly IUserCommandService _userCommandService;

    public UsersController(IUserQueryService userQueryService, IUserCommandService userCommandService)
    {
        _userQueryService = userQueryService;
        _userCommandService = userCommandService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        if (!IsOwnProfile(id))
            return Forbid();

        var user = await _userQueryService.Handle(new GetUserByIdQuery(id));

        if (user is null)
            return NotFound();

        var resource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(resource);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileResource resource)
    {
        if (!IsOwnProfile(id))
            return Forbid();

        try
        {
            var command = UpdateProfileCommandFromResourceAssembler.ToCommandFromResource(id, resource);
            var user = await _userCommandService.Handle(command);
            return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Evita IDOR: un usuario solo puede ver/editar su propio perfil.</summary>
    private bool IsOwnProfile(int id)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var authenticatedUserId) && authenticatedUserId == id;
    }
}
