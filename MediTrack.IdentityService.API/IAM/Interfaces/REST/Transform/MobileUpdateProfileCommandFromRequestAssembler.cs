using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

public static class MobileUpdateProfileCommandFromRequestAssembler
{
    public static UpdateProfileCommand ToCommandFromRequest(int userId, MobileUpdateProfileRequest request)
    {
        // ProfilePhotoUrl se ignora deliberadamente: solo los endpoints de foto
        // pueden modificar esa referencia.
        return new UpdateProfileCommand(
            userId,
            FullName: request.Nombre,
            Email: request.Email,
            Institution: request.Institucion,
            PhoneNumber: request.PhoneNumber
        );
    }
}
