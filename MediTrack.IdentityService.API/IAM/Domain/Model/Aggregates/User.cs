using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;

namespace MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;

public class User
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public string? Dni { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
    }

    public User(string email, string passwordHash, string fullName, UserRole role, string? dni = null, DateTime? dateOfBirth = null)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        Dni = dni;
        DateOfBirth = dateOfBirth;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string email, string? dni = null, DateTime? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new Exception("Full name is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new Exception("Email is required");

        FullName = fullName;
        Email = email;

        // Dni/DateOfBirth rara vez cambian y no siempre vienen en el request
        // (p. ej. un cliente que solo corrige el nombre) -- omitirlos no debe
        // borrar el valor ya guardado.
        if (dni is not null)
            Dni = dni;

        if (dateOfBirth is not null)
            DateOfBirth = dateOfBirth;
    }
}
