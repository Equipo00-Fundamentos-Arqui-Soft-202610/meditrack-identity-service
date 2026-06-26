using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        if (!Enum.TryParse<UserRole>(resource.Role, true, out var role))
            throw new Exception("Invalid role. Allowed values: Patient, TechnicalStaff");

        return new SignUpCommand(
            resource.Email,
            resource.Password,
            resource.FullName,
            role
        );
    }
}
