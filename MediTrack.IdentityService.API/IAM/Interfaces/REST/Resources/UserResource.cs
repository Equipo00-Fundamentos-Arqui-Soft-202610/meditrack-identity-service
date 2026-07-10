namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record UserResource(
    int Id,
    string Email,
    string FullName,
    string Role,
    DateTime CreatedAt,
    string? Dni,
    DateTime? DateOfBirth
);
