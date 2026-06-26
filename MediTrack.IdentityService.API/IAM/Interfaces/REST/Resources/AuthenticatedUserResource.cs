namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record AuthenticatedUserResource(
    int Id,
    string Email,
    string FullName,
    string Role,
    string Token
);
