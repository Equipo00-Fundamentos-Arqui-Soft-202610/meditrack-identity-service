namespace MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

public record SignInCommand(
    string Email,
    string Password,
    string? ClientType = null
);
