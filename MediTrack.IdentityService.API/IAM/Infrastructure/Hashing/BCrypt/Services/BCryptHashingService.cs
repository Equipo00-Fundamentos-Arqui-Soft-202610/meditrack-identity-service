using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;

namespace MediTrack.IdentityService.API.IAM.Infrastructure.Hashing.BCrypt.Services;

public class BCryptHashingService : IHashingService
{
    public string HashPassword(string password)
    {
        return global::BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return global::BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
