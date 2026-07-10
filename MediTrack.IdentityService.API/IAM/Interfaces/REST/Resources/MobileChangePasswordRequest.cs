namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;

public record MobileChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
