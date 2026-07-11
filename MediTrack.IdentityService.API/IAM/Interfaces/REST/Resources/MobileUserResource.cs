namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record MobileUserResource(
    int Id,
    string Nombre,
    string Email,
    string Rol,
    string? Institucion,
    string? PhoneNumber,
    string? ProfilePhotoUrl,
    string? Dni,
    DateTime? FechaNacimiento
);
