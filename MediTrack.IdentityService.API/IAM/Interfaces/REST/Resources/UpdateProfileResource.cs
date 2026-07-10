namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record UpdateProfileResource(
    string FullName,
    string Email
);
