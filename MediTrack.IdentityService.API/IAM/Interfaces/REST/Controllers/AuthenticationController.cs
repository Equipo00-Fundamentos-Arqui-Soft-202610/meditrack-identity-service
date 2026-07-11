using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;

    public AuthenticationController(IUserCommandService userCommandService)
    {
        _userCommandService = userCommandService;
    }

    [AllowAnonymous]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource)
    {
        try
        {
            var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);

            var (user, token) = await _userCommandService.Handle(command);

            var authenticatedUser = AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(user, token);

            return Created(string.Empty, authenticatedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        try
        {
            var clientType = Request.Headers["X-Client-Type"].FirstOrDefault();
            var command = SignInCommandFromResourceAssembler.ToCommandFromResource(resource, clientType);

            var (user, token) = await _userCommandService.Handle(command);

            var authenticatedUser = AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(user, token);

            return Ok(authenticatedUser);
        }
        catch (Exception ex)
        {
            var statusCode = ex.Message.Contains("portal es exclusivo")
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status401Unauthorized;
            return StatusCode(statusCode, new { message = ex.Message });
        }
    }
}
