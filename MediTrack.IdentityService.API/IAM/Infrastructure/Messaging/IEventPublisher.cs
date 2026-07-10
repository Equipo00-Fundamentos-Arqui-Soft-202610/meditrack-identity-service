namespace MediTrack.IdentityService.API.IAM.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(string routingKey, object payload);
}
