using MediTrack.IdentityService.API.IAM.Application.Internal.CommandServices;
using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;
using MediTrack.IdentityService.API.IAM.Application.Internal.QueryServices;
using MediTrack.IdentityService.API.IAM.Domain.Repositories;
using MediTrack.IdentityService.API.IAM.Domain.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using MediTrack.IdentityService.API.IAM.Infrastructure.Pipeline.Extensions;
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

// JWT authentication

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddJwtAuthentication(builder.Configuration);

// Dependency Injection

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IHashingService, BCryptHashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

// Create the database schema on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
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
