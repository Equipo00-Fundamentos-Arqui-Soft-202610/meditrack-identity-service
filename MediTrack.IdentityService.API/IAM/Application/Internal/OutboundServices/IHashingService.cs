namespace MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;

/// <summary>
/// Servicio de hashing de contraseñas. La contraseña en claro nunca se persiste:
/// solo se almacena su hash (CON-09, Ley N° 29733 de protección de datos personales).
/// </summary>
public interface IHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
