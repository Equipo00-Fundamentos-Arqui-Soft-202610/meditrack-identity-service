using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource, string? clientType = null)
    {
        return new SignInCommand(resource.Email, resource.Password, clientType);
    }
}
