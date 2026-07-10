namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record SignUpResource(
    string Email,
    string Password,
    string FullName,
    string Role,
    string? Dni = null,
    DateTime? DateOfBirth = null
);
