namespace MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;

/// <summary>
/// Roles del sistema, alineados con los dos segmentos objetivo del informe
/// (Paciente y Personal Técnico). El claim de rol viaja en el JWT y es la base
/// de la autorización por rol del API Gateway (AC-01).
/// </summary>
public enum UserRole
{
    Patient,
    TechnicalStaff
}
