using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;

namespace MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;

public class User
{
    public const int MaxPhoneNumberLength = 30;
    public const int MaxProfilePhotoUrlLength = 1024;

    public int Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FullName { get; private set; }
    public string? Dni { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? Institution { get; private set; }
    public int? PatientId { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? ProfilePhotoUrl { get; private set; }

    protected User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FullName = string.Empty;
    }

    public User(
        string email,
        string passwordHash,
        string fullName,
        UserRole role,
        string? dni = null,
        DateTime? dateOfBirth = null,
        string? institution = null)
    {
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        Dni = dni;
        DateOfBirth = dateOfBirth;
        Institution = institution;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string? fullName = null, string? email = null, string? dni = null, DateTime? dateOfBirth = null)
    {
        if (fullName is not null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("Full name is required");

            FullName = fullName;
        }

        if (email is not null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email is required");

            Email = email;
        }

        // Dni/DateOfBirth rara vez cambian y no siempre vienen en el request
        // (p. ej. un cliente que solo corrige el nombre) -- omitirlos no debe
        // borrar el valor ya guardado.
        if (dni is not null)
            Dni = dni;

        if (dateOfBirth is not null)
            DateOfBirth = dateOfBirth;
    }

    public void UpdateInstitution(string? institution)
    {
        Institution = institution;
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        if (phoneNumber is not null && phoneNumber.Length > MaxPhoneNumberLength)
            throw new Exception($"Phone number must be at most {MaxPhoneNumberLength} characters long");

        PhoneNumber = phoneNumber;
    }

    public void UpdateProfilePhotoUrl(string? profilePhotoUrl)
    {
        if (profilePhotoUrl is not null && profilePhotoUrl.Length > MaxProfilePhotoUrlLength)
            throw new Exception($"Profile photo reference must be at most {MaxProfilePhotoUrlLength} characters long");

        ProfilePhotoUrl = profilePhotoUrl;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new Exception("Password hash is required");

        PasswordHash = passwordHash;
    }
}
