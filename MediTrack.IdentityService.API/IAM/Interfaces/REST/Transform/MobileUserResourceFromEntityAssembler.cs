using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class MobileUserResourceFromEntityAssembler
{
    public static MobileUserResource ToResourceFromEntity(User user)
    {
        return new MobileUserResource(
            user.Id,
            user.FullName,
            user.Email,
            MobileRoleMapper.ToMobileRole(user.Role),
            user.Institution,
            user.PhoneNumber,
            user.ProfilePhotoUrl
        );
    }
}
