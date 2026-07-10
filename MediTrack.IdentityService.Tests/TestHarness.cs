using System.Security.Claims;
using MediTrack.IdentityService.API.IAM.Application.Internal.CommandServices;
using MediTrack.IdentityService.API.IAM.Application.Internal.QueryServices;
using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Messaging;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using MediTrack.IdentityService.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using MediTrack.IdentityService.API.IAM.Infrastructure.Tokens.JWT.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MediTrack.IdentityService.Tests;

/// Construye instancias reales de los servicios de aplicación (UserCommandService,
/// UserQueryService, BCryptHashingService, TokenService, OutboxEventPublisher) sobre
/// un AppDbContext InMemory, para probar los controllers mobile sin mocks de por medio.
internal static class TestHarness
{
    public static AppDbContext NewContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    public static IUserCommandService BuildCommandService(AppDbContext context)
    {
        var repository = new UserRepository(context);
        var hashing = new BCryptHashingService();
        var tokenSettings = Options.Create(new TokenSettings
        {
            Issuer = "meditrack-tests",
            Audience = "meditrack-tests",
            Key = "test-signing-key-at-least-32-characters-long!!",
            ExpiresInHours = 1
        });
        var tokenService = new TokenService(tokenSettings);
        IEventPublisher eventPublisher = new OutboxEventPublisher(context);

        return new UserCommandService(repository, hashing, tokenService, eventPublisher);
    }

    public static IUserQueryService BuildQueryService(AppDbContext context) =>
        new UserQueryService(new UserRepository(context));

    public static ClaimsPrincipal BuildPrincipal(int userId) =>
        new(new ClaimsIdentity(new[] { new Claim("sub", userId.ToString()) }, "TestAuth"));
}
