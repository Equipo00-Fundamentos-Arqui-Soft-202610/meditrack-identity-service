using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;

namespace MediTrack.IdentityService.API.IAM.Domain.Services;

public interface IUserCommandService
{
    Task<(User user, string token)> Handle(SignUpCommand command);
    Task<(User user, string token)> Handle(SignInCommand command);
}
