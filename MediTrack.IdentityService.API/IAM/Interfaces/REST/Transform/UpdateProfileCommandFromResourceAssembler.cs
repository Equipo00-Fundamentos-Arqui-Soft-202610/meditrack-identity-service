using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class UpdateProfileCommandFromResourceAssembler
{
    public static UpdateProfileCommand ToCommandFromResource(int userId, UpdateProfileResource resource)
    {
        return new UpdateProfileCommand(userId, resource.FullName, resource.Email, resource.Dni, resource.DateOfBirth);
    }
}
