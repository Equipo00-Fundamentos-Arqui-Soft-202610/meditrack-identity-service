namespace MediTrack.IdentityService.API.IAM.Application.OutboundEvents;

/// <summary>
/// Se publica cuando un usuario con rol Patient completa el registro (US-IAM-RF1).
/// Los demás bounded contexts (p. ej. Treatment) lo consumen para mantener su propia
/// proyección local de pacientes válidos, evitando llamadas síncronas entre servicios.
/// </summary>
public sealed record PacienteRegistradoEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    int PatientId,
    string FullName,
    string Email,
    string? Dni = null,
    DateTime? DateOfBirth = null
);
