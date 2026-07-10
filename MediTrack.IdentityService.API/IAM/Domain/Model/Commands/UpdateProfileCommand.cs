namespace MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

public record UpdateProfileCommand(int UserId, string FullName, string Email);
