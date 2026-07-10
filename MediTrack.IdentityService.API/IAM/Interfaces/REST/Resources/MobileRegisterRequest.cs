namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record MobileRegisterRequest(
    string Nombre,
    string Email,
    string Password,
    string Rol,
    string? Institucion = null
);
