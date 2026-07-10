using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class MobileAuthResponseFromEntityAssembler
{
    public static MobileAuthResponse ToResourceFromEntity(User user, string token)
    {
        return new MobileAuthResponse(token, MobileUserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }
}
