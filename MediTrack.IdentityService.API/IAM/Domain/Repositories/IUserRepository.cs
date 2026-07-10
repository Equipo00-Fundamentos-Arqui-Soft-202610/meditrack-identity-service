using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;

namespace MediTrack.IdentityService.API.IAM.Domain.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<User?> FindByIdAsync(int id);
    Task<User?> FindByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
}
