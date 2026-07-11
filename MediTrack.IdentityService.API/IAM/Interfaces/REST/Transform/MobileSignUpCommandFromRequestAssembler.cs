using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class MobileSignUpCommandFromRequestAssembler
{
    public static SignUpCommand ToCommandFromRequest(MobileRegisterRequest request)
    {
        var role = MobileRoleMapper.ToDomainRole(request.Rol);

        return new SignUpCommand(
            request.Email,
            request.Password,
            request.Nombre,
            role,
            Dni: request.Dni,
            DateOfBirth: request.FechaNacimiento,
            Institution: request.Institucion
        );
    }
}
