using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Domain.Model.Queries;
using MediTrack.IdentityService.API.IAM.Domain.Repositories;
using MediTrack.IdentityService.API.IAM.Domain.Services;

namespace MediTrack.IdentityService.API.IAM.Application.Internal.QueryServices;

public class UserQueryService : IUserQueryService
{
    private readonly IUserRepository _userRepository;

    public UserQueryService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await _userRepository.FindByIdAsync(query.Id);
    }

    public async Task<User?> Handle(GetUserByEmailQuery query)
    {
        return await _userRepository.FindByEmailAsync(query.Email);
    }
}
