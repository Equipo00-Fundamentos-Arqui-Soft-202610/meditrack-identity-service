using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;

namespace MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;

public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
    }

    public User(string email, string passwordHash, string fullName, UserRole role)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new Exception("Full name is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new Exception("Email is required");

        FullName = fullName;
        Email = email;
    }
}
