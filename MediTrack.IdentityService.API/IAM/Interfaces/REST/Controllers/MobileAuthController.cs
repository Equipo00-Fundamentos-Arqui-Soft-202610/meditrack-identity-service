using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;

/// <summary>
/// Capa de compatibilidad para MediTrack-Mobile: expone el contrato que el
/// Gateway reenvía en /identity/api/v1/auth/*, reutilizando el mismo
/// IUserCommandService (BCrypt, Outbox, TokenService) que AuthenticationController.
/// No reemplaza ni modifica los endpoints de api/v1/authentication.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class MobileAuthController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;

    public MobileAuthController(IUserCommandService userCommandService)
    {
        _userCommandService = userCommandService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] MobileLoginRequest request)
    {
        try
        {
            var command = MobileSignInCommandFromRequestAssembler.ToCommandFromRequest(request);
            var (user, token) = await _userCommandService.Handle(command);

            return Ok(MobileAuthResponseFromEntityAssembler.ToResourceFromEntity(user, token));
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] MobileRegisterRequest request)
    {
        try
        {
            var command = MobileSignUpCommandFromRequestAssembler.ToCommandFromRequest(request);
            var (user, token) = await _userCommandService.Handle(command);

            return Created(string.Empty, MobileAuthResponseFromEntityAssembler.ToResourceFromEntity(user, token));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
