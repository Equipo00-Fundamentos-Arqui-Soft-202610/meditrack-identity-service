using MediTrack.IdentityService.API.IAM.Application.Internal.CommandServices;
using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;
using MediTrack.IdentityService.API.IAM.Application.Internal.QueryServices;
using MediTrack.IdentityService.API.IAM.Domain.Repositories;
using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Messaging;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using MediTrack.IdentityService.API.IAM.Infrastructure.Pipeline.Extensions;
using MediTrack.IdentityService.API.IAM.Infrastructure.Storage;
using MediTrack.IdentityService.API.IAM.Infrastructure.Tokens.JWT.Configuration;
using MediTrack.IdentityService.API.IAM.Infrastructure.Tokens.JWT.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Database

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySQL(connectionString);
});

// JWT authentication -- valores reales vía user-secrets en desarrollo, vía
// variables de entorno en producción. Nunca en appsettings.json (ver README).
builder.Services.AddOptions<TokenSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key es obligatorio")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer es obligatorio")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience es obligatorio")
    .ValidateOnStart();

builder.Services.AddJwtAuthentication(builder.Configuration);

// Dependency Injection

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IHashingService, BCryptHashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Compatibilidad con MediTrack-Mobile: foto de perfil en almacenamiento local,
// en una carpeta privada (no expuesta vía UseStaticFiles). Ruta configurable
// mediante ProfilePhotos:StoragePath; ver README para consideraciones de
// persistencia en despliegues sin disco persistente.
builder.Services.Configure<ProfilePhotoStorageOptions>(builder.Configuration.GetSection(ProfilePhotoStorageOptions.SectionName));
builder.Services.AddScoped<IProfilePhotoStorage, LocalProfilePhotoStorage>();

// Messaging (CON-05): publica PacienteRegistrado para que otros bounded contexts
// mantengan su propia proyección local, sin llamadas síncronas entre servicios.
// Patrón Outbox: el evento se persiste en la misma base de datos que el cambio
// de dominio y se entrega a RabbitMQ en background (no se pierde si el broker
// está caído justo al publicar, ni tumba el sign-up cuando eso pasa).
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddScoped<IEventPublisher, OutboxEventPublisher>();
builder.Services.AddHostedService<OutboxDispatcherHostedService>();

var app = builder.Build();

// Create/update the database schema on startup.
//
// Este servicio no tenía migraciones de EF Core (usaba EnsureCreatedAsync,
// que solo crea el esquema completo si la base de datos NO existe todavía y
// no hace nada si ya existe). La base de datos de producción ya existe (con
// filas reales en `users`), así que EnsureCreatedAsync jamás habría creado la
// nueva tabla `outbox_message` ahí. Por eso: si la base de datos es nueva
// (dev/test) se sigue usando EnsureCreatedAsync tal como antes; si ya existe
// (producción) se aplica la migración aditiva `AddOutboxMessage`, que
// únicamente crea `outbox_message` y no toca `users`.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var databaseAlreadyExists = await db.Database.CanConnectAsync();

    if (!databaseAlreadyExists)
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "No se pudo preparar el esquema de base de datos al arrancar.");
}

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
