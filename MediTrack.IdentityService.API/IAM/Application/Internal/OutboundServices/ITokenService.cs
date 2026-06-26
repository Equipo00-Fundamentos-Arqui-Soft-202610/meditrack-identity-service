using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;

namespace MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;

/// <summary>
/// Genera el JSON Web Token (JWT) firmado para un usuario autenticado.
/// El token incluye el claim de rol que el API Gateway usa para autorizar (AC-01).
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}
