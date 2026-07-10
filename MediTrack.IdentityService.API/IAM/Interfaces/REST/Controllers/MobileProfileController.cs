using System.Security.Claims;
using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;
using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Domain.Model.Queries;
using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;

/// <summary>
/// Capa de compatibilidad para MediTrack-Mobile: expone el perfil autenticado
/// que el Gateway reenvía en /identity/api/v1/profile/*. El usuario siempre se
/// resuelve desde el claim sub/NameIdentifier del JWT, nunca desde un id que
/// mande el cliente. Reutiliza IUserCommandService/IUserQueryService, por lo
/// que toda mutación pasa por la capa de aplicación (BCrypt, Outbox, reglas de
/// dominio de User) igual que UsersController.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/profile")]
public class MobileProfileController : ControllerBase
{
    private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedPhotoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp"
    };

    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

    private readonly IUserQueryService _userQueryService;
    private readonly IUserCommandService _userCommandService;
    private readonly IProfilePhotoStorage _photoStorage;

    public MobileProfileController(
        IUserQueryService userQueryService,
        IUserCommandService userCommandService,
        IProfilePhotoStorage photoStorage)
    {
        _userQueryService = userQueryService;
        _userCommandService = userCommandService;
        _photoStorage = photoStorage;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _userQueryService.Handle(new GetUserByIdQuery(userId.Value));
        if (user is null) return NotFound();

        return Ok(MobileUserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] MobileUpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var command = MobileUpdateProfileCommandFromRequestAssembler.ToCommandFromRequest(userId.Value, request);
            var user = await _userCommandService.Handle(command);

            return Ok(MobileUserResourceFromEntityAssembler.ToResourceFromEntity(user));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] MobileChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            await _userCommandService.Handle(new ChangePasswordCommand(userId.Value, request.CurrentPassword, request.NewPassword));
            return Ok(new { message = "Contraseña actualizada correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// Sube o reemplaza la foto de perfil del usuario autenticado. Guarda primero
    /// el archivo nuevo, persiste la referencia y solo después borra el archivo
    /// anterior (si existía). Si persistir la referencia falla, borra el archivo
    /// nuevo para no dejar huérfanos.
    [HttpPost("photo")]
    [RequestSizeLimit(MaxPhotoSizeBytes + 1024)]
    public async Task<IActionResult> UploadPhoto(IFormFile? photo)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (photo == null || photo.Length == 0)
            return BadRequest(new { message = "Debes adjuntar una imagen." });

        if (photo.Length > MaxPhotoSizeBytes)
            return BadRequest(new { message = "La imagen no debe superar 5MB." });

        var extension = Path.GetExtension(photo.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedPhotoExtensions.Contains(extension))
            return BadRequest(new { message = "Formato no soportado. Usa JPG, PNG o WEBP." });

        if (string.IsNullOrWhiteSpace(photo.ContentType) || !AllowedPhotoContentTypes.Contains(photo.ContentType))
            return BadRequest(new { message = "El archivo no parece ser una imagen válida." });

        var user = await _userQueryService.Handle(new GetUserByIdQuery(userId.Value));
        if (user is null) return NotFound();

        var previousFileName = user.ProfilePhotoUrl;

        string fileName;
        await using (var stream = photo.OpenReadStream())
        {
            fileName = await _photoStorage.SaveAsync(stream, extension.ToLowerInvariant());
        }

        try
        {
            var updated = await _userCommandService.Handle(new UpdateProfilePhotoCommand(userId.Value, fileName));

            if (!string.IsNullOrWhiteSpace(previousFileName))
                _photoStorage.Delete(previousFileName);

            return Ok(MobileUserResourceFromEntityAssembler.ToResourceFromEntity(updated));
        }
        catch
        {
            _photoStorage.Delete(fileName);
            throw;
        }
    }

    /// Devuelve la foto del usuario autenticado. No acepta nombres de archivo ni
    /// IDs desde el cliente; el parámetro `v` que envía el mobile para evitar
    /// caché no se usa aquí y puede ignorarse con seguridad.
    [HttpGet("photo")]
    public async Task<IActionResult> GetPhoto()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _userQueryService.Handle(new GetUserByIdQuery(userId.Value));
        if (user is null || string.IsNullOrWhiteSpace(user.ProfilePhotoUrl)) return NotFound();

        var stream = _photoStorage.OpenRead(user.ProfilePhotoUrl);
        if (stream is null) return NotFound();

        return File(stream, _photoStorage.GetContentType(user.ProfilePhotoUrl));
    }

    /// Idempotente: si el usuario no tiene foto, simplemente devuelve su perfil actual.
    [HttpDelete("photo")]
    public async Task<IActionResult> DeletePhoto()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _userQueryService.Handle(new GetUserByIdQuery(userId.Value));
        if (user is null) return NotFound();

        if (string.IsNullOrWhiteSpace(user.ProfilePhotoUrl))
            return Ok(MobileUserResourceFromEntityAssembler.ToResourceFromEntity(user));

        var fileToDelete = user.ProfilePhotoUrl;
        var updated = await _userCommandService.Handle(new UpdateProfilePhotoCommand(userId.Value, null));
        _photoStorage.Delete(fileToDelete);

        return Ok(MobileUserResourceFromEntityAssembler.ToResourceFromEntity(updated));
    }

    private int? GetUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}
