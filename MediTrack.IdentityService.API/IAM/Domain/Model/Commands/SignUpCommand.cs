using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;

namespace MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

public record SignUpCommand(
    string Email,
    string Password,
    string FullName,
    UserRole Role,
    string? Dni = null,
    DateTime? DateOfBirth = null
);
