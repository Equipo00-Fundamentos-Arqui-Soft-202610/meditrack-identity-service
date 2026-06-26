namespace MediTrack.IdentityService.API.IAM.Infrastructure.Tokens.JWT.Configuration;

/// <summary>
/// Configuración del JWT, enlazada desde la sección "Jwt" de appsettings.
/// Los valores (Issuer, Audience, Key) deben ser IDÉNTICOS en todos los
/// microservicios para que la validación de firma compartida funcione (CON-04).
/// </summary>
public class TokenSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiresInHours { get; set; } = 8;
}
