using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;
using MediTrack.IdentityService.API.IAM.Infrastructure.Hashing.BCrypt.Services;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.IdentityService.Tests;

/// Cubre GET/PUT api/v1/profile y PUT api/v1/profile/password: el usuario
/// siempre se resuelve desde el claim sub del JWT (nunca desde un id enviado
/// por el cliente), lo que impide por diseño consultar/editar el perfil de otro.
public class MobileProfileTests
{
    private static MobileProfileController BuildController(AppDbContext context, int userId, FakeProfilePhotoStorage? storage = null)
    {
        return new MobileProfileController(
            TestHarness.BuildQueryService(context),
            TestHarness.BuildCommandService(context),
            storage ?? new FakeProfilePhotoStorage())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = TestHarness.BuildPrincipal(userId) }
            }
        };
    }

    [Fact]
    public async Task GetProfile_ResuelveUsuarioDesdeElJwt()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id);

        var result = await controller.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.Equal(user.Id, resource.Id);
        Assert.Equal("Ana", resource.Nombre);
        Assert.Equal("paciente", resource.Rol);
    }

    [Fact]
    public async Task GetProfile_UnUsuarioNoPuedeConsultarElPerfilDeOtro()
    {
        await using var context = TestHarness.NewContext();
        var ana = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        var luis = new User("luis@test.com", "hash", "Luis", UserRole.Patient);
        context.Users.AddRange(ana, luis);
        await context.SaveChangesAsync();

        var controllerForAna = BuildController(context, ana.Id);

        var result = await controllerForAna.GetProfile();

        var ok = Assert.IsType<OkObjectResult>(result);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.Equal(ana.Email, resource.Email);
        Assert.NotEqual(luis.Email, resource.Email);
    }

    [Fact]
    public async Task UpdateProfile_ActualizaNombreEmailInstitucionYTelefono()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id);

        var result = await controller.UpdateProfile(new MobileUpdateProfileRequest(
            Nombre: "Ana Actualizada",
            Email: "ana.nueva@test.com",
            Institucion: "Clinica Central",
            PhoneNumber: "999999999"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.Equal("Ana Actualizada", resource.Nombre);
        Assert.Equal("ana.nueva@test.com", resource.Email);
        Assert.Equal("Clinica Central", resource.Institucion);
        Assert.Equal("999999999", resource.PhoneNumber);
    }

    [Fact]
    public async Task UpdateProfile_UnUsuarioNoPuedeModificarElPerfilDeOtro()
    {
        await using var context = TestHarness.NewContext();
        var ana = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        var luis = new User("luis@test.com", "hash", "Luis", UserRole.Patient);
        context.Users.AddRange(ana, luis);
        await context.SaveChangesAsync();

        var controllerForAna = BuildController(context, ana.Id);
        await controllerForAna.UpdateProfile(new MobileUpdateProfileRequest(Nombre: "Ana Hackeada"));

        var luisRecargado = await context.Users.FindAsync(luis.Id);
        Assert.Equal("Luis", luisRecargado!.FullName);
    }

    [Fact]
    public async Task UpdateProfile_ConEmailDeOtroUsuario_DevuelveBadRequestYNoActualizaNada()
    {
        await using var context = TestHarness.NewContext();
        var ana = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        var luis = new User("luis@test.com", "hash", "Luis", UserRole.Patient);
        context.Users.AddRange(ana, luis);
        await context.SaveChangesAsync();

        var controller = BuildController(context, ana.Id);

        var result = await controller.UpdateProfile(new MobileUpdateProfileRequest(Email: "luis@test.com"));

        Assert.IsType<BadRequestObjectResult>(result);
        var anaRecargada = await context.Users.FindAsync(ana.Id);
        Assert.Equal("ana@test.com", anaRecargada!.Email);
    }

    [Fact]
    public async Task UpdateProfile_IgnoraProfilePhotoUrlEnviadoPorElCliente()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        user.UpdateProfilePhotoUrl("foto-legitima.jpg");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id);

        var result = await controller.UpdateProfile(new MobileUpdateProfileRequest(ProfilePhotoUrl: "foto-hackeada.jpg"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.Equal("foto-legitima.jpg", resource.ProfilePhotoUrl);
    }

    [Fact]
    public async Task UpdateProfile_NoPermiteCambiarElRolDelUsuario()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id);
        await controller.UpdateProfile(new MobileUpdateProfileRequest(Nombre: "Ana"));

        var recargado = await context.Users.FindAsync(user.Id);
        Assert.Equal(UserRole.Patient, recargado!.Role);
    }

    [Fact]
    public async Task ChangePassword_ConPasswordActualIncorrecta_DevuelveBadRequest()
    {
        await using var context = TestHarness.NewContext();
        var hashing = new BCryptHashingService();
        var user = new User("ana@test.com", hashing.HashPassword("correcta123"), "Ana", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id);

        var result = await controller.ChangePassword(new MobileChangePasswordRequest("incorrecta", "nueva12345"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_ConPasswordActualCorrecta_ActualizaElHashManteniendoBCrypt()
    {
        await using var context = TestHarness.NewContext();
        var hashing = new BCryptHashingService();
        var user = new User("ana@test.com", hashing.HashPassword("correcta123"), "Ana", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id);

        var result = await controller.ChangePassword(new MobileChangePasswordRequest("correcta123", "nueva12345"));

        Assert.IsType<OkObjectResult>(result);
        var updated = await context.Users.FindAsync(user.Id);
        Assert.True(hashing.VerifyPassword("nueva12345", updated!.PasswordHash));
        Assert.StartsWith("$2", updated.PasswordHash);
    }
}
