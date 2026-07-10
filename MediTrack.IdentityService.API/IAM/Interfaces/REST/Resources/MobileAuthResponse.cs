namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record MobileAuthResponse(
    string AccessToken,
    MobileUserResource Usuario
);
