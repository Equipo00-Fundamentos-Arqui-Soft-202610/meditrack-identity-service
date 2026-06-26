using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;

namespace MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;

public interface ITokenService
{
    string GenerateToken(User user);
}
