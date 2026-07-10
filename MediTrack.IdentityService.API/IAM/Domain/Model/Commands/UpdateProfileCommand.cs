namespace MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

public record UpdateProfileCommand(
    int UserId,
    string FullName,
    string Email,
    string? Dni = null,
    DateTime? DateOfBirth = null
);
