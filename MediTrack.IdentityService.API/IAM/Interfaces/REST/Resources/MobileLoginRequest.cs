namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record MobileLoginRequest(
    string Email,
    string Password
);
