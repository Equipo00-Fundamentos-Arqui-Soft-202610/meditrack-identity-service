namespace MediTrack.IdentityService.API.IAM.Infrastructure.Tokens.JWT.Configuration;

public class TokenSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiresInHours { get; set; } = 8;
}
