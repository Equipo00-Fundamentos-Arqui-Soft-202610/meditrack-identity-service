using System.Text.Json;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;

namespace MediTrack.IdentityService.API.IAM.Infrastructure.Messaging;

/// <summary>
/// Publica vía el patrón Outbox: en vez de mandar a RabbitMQ de inmediato (y
/// fallar todo el sign-up si el broker está caído justo en ese momento, como
/// pasaba antes), persiste el mensaje y deja que
/// <see cref="OutboxDispatcherHostedService"/> lo entregue en background con
/// reintentos.
/// </summary>
public sealed class OutboxEventPublisher : IEventPublisher
{
    private readonly AppDbContext _context;

    public OutboxEventPublisher(AppDbContext context)
    {
        _context = context;
    }

    public async Task PublishAsync(string routingKey, object payload)
    {
        var message = new OutboxMessage
        {
            EventType = routingKey,
            Payload = JsonSerializer.Serialize(payload),
            OccurredAtUtc = DateTime.UtcNow
        };

        _context.OutboxMessages.Add(message);
        await _context.SaveChangesAsync();
    }
}
