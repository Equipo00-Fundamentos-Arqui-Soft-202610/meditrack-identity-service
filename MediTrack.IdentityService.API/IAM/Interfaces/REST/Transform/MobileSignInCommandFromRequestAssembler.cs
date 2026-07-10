using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class MobileSignInCommandFromRequestAssembler
{
    public static SignInCommand ToCommandFromRequest(MobileLoginRequest request)
    {
        return new SignInCommand(request.Email, request.Password);
    }
}
