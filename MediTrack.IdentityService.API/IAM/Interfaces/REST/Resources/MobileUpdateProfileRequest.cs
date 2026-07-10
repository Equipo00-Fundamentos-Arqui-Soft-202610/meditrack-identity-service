namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

/// <summary>
/// ProfilePhotoUrl viaja en el contrato del mobile pero se ignora deliberadamente:
/// la foto de perfil solo se puede modificar a través de los endpoints /profile/photo.
/// </summary>
public record MobileUpdateProfileRequest(
    string? Nombre = null,
    string? Email = null,
    string? Institucion = null,
    string? PhoneNumber = null,
    string? ProfilePhotoUrl = null
);
