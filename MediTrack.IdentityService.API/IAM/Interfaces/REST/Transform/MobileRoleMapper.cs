using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;

namespace MediTrack.IdentityService.API.IAM.Interfaces.REST.Transform;

/// <summary>
/// Traduce entre los nombres de rol que usa el contrato del mobile ("paciente",
/// "patient", "technicalstaff", "doctor", "admin") y el enum UserRole del
/// dominio, sin modificarlo. No afecta al enum ni a los endpoints antiguos,
/// que siguen usando Enum.TryParse contra "Patient"/"TechnicalStaff".
/// </summary>
public static class MobileRoleMapper
{
    public static UserRole ToDomainRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new Exception("Invalid role. Allowed values: paciente, patient, technicalstaff, doctor, admin");

        return role.Trim().ToLowerInvariant() switch
        {
            "paciente" or "patient" => UserRole.Patient,
            "technicalstaff" or "doctor" or "admin" => UserRole.TechnicalStaff,
            _ => throw new Exception("Invalid role. Allowed values: paciente, patient, technicalstaff, doctor, admin")
        };
    }

    public static string ToMobileRole(UserRole role) => role switch
    {
        UserRole.Patient => "paciente",
        UserRole.TechnicalStaff => "TechnicalStaff",
        _ => role.ToString()
    };
}
