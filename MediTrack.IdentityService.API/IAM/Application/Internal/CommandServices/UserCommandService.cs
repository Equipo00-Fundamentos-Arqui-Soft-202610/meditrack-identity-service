using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;
using MediTrack.IdentityService.API.IAM.Application.OutboundEvents;
using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Domain.Model.Commands;
using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;
using MediTrack.IdentityService.API.IAM.Domain.Repositories;
using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Messaging;

namespace MediTrack.IdentityService.API.IAM.Application.Internal.CommandServices;

public class UserCommandService : IUserCommandService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashingService _hashingService;
    private readonly ITokenService _tokenService;
    private readonly IEventPublisher _eventPublisher;

    public UserCommandService(
        IUserRepository userRepository,
        IHashingService hashingService,
        ITokenService tokenService,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _hashingService = hashingService;
        _tokenService = tokenService;
        _eventPublisher = eventPublisher;
    }

    public async Task<(User user, string token)> Handle(SignUpCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new Exception("Email is required");

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
            throw new Exception("Password must be at least 8 characters long");

        if (string.IsNullOrWhiteSpace(command.FullName))
            throw new Exception("Full name is required");

        if (await _userRepository.ExistsByEmailAsync(command.Email))
            throw new Exception("Email already registered");

        var passwordHash = _hashingService.HashPassword(command.Password);
        var user = new User(command.Email, passwordHash, command.FullName, command.Role);

        await _userRepository.AddAsync(user);

        if (user.Role == UserRole.Patient)
        {
            await _eventPublisher.PublishAsync("PacienteRegistrado", new PacienteRegistradoEvent(
                Guid.NewGuid(), DateTime.UtcNow, user.Id, user.FullName, user.Email));
        }

        var token = _tokenService.GenerateToken(user);
        return (user, token);
    }

    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await _userRepository.FindByEmailAsync(command.Email);

        if (user is null || !_hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        var token = _tokenService.GenerateToken(user);
        return (user, token);
    }

    public async Task<User> Handle(UpdateProfileCommand command)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId)
            ?? throw new Exception("User not found");

        if (!string.Equals(user.Email, command.Email, StringComparison.OrdinalIgnoreCase)
            && await _userRepository.ExistsByEmailAsync(command.Email))
            throw new Exception("Email already registered");

        user.UpdateProfile(command.FullName, command.Email);
        await _userRepository.UpdateAsync(user);

        return user;
    }
}
