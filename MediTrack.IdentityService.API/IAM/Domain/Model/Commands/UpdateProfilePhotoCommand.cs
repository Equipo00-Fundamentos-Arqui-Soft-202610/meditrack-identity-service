namespace MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

public record UpdateProfilePhotoCommand(
    int UserId,
    string? ProfilePhotoUrl
);
