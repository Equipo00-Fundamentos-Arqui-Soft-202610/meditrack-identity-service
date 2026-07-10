namespace MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

public record UpdateProfileCommand(
    int UserId,
    string? FullName = null,
    string? Email = null,
    string? Dni = null,
    DateTime? DateOfBirth = null,
    string? Institution = null,
    string? PhoneNumber = null
);
