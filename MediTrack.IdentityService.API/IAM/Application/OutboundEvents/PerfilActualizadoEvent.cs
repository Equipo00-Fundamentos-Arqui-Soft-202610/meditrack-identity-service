namespace MediTrack.IdentityService.API.IAM.Application.OutboundEvents;

/// <summary>
/// Se publica cuando un usuario con rol Patient edita su perfil (nombre, email,
/// DNI o fecha de nacimiento) después del registro. Mantiene sincronizadas las
/// proyecciones locales de otros bounded contexts (p. ej. Treatment) que se
/// alimentaron inicialmente de <see cref="PacienteRegistradoEvent"/>.
/// </summary>
public sealed record PerfilActualizadoEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    int PatientId,
    string FullName,
    string Email,
    string? Dni,
    DateTime? DateOfBirth
);
