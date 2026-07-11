using MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.IdentityService.Tests;

/// Cubre el contrato de auth que consume MediTrack-Mobile (api/v1/auth/login
/// y api/v1/auth/register), reutilizando IUserCommandService (BCrypt, Outbox,
/// TokenService) sin duplicar lógica de contraseñas.
public class MobileAuthTests
{
    [Fact]
    public async Task Register_ConContratoMobile_CreaPacienteYDevuelveAccessTokenYUsuario()
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));

        var result = await controller.Register(new MobileRegisterRequest("Ana Paciente", "ana@test.com", "password123", "paciente"));

        var created = Assert.IsType<CreatedResult>(result);
        var body = Assert.IsType<MobileAuthResponse>(created.Value);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal("paciente", body.Usuario.Rol);
        Assert.Equal("ana@test.com", body.Usuario.Email);
    }

    [Theory]
    [InlineData("patient")]
    [InlineData("PACIENTE")]
    public async Task Register_ConSinonimosDePaciente_NormalizaARolPaciente(string rol)
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));

        var result = await controller.Register(new MobileRegisterRequest("Ana", "ana2@test.com", "password123", rol));

        var created = Assert.IsType<CreatedResult>(result);
        var body = Assert.IsType<MobileAuthResponse>(created.Value);
        Assert.Equal("paciente", body.Usuario.Rol);
    }

    [Theory]
    [InlineData("technicalstaff")]
    [InlineData("doctor")]
    [InlineData("admin")]
    public async Task Register_ConSinonimosDeStaff_NormalizaARolTechnicalStaff(string rol)
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));

        var result = await controller.Register(new MobileRegisterRequest("Dr. Luis", $"{rol}@test.com", "password123", rol));

        var created = Assert.IsType<CreatedResult>(result);
        var body = Assert.IsType<MobileAuthResponse>(created.Value);
        Assert.Equal("TechnicalStaff", body.Usuario.Rol);
    }

    [Fact]
    public async Task Register_ConDniYFechaNacimiento_LosPersisteYDevuelveEnUsuario()
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));

        var result = await controller.Register(new MobileRegisterRequest(
            "Ana Paciente", "ana-dni@test.com", "password123", "paciente",
            Dni: "12345678", FechaNacimiento: new DateTime(1995, 4, 12)));

        var created = Assert.IsType<CreatedResult>(result);
        var body = Assert.IsType<MobileAuthResponse>(created.Value);
        Assert.Equal("12345678", body.Usuario.Dni);
        Assert.Equal(new DateTime(1995, 4, 12), body.Usuario.FechaNacimiento);
    }

    [Fact]
    public async Task Register_ConEmailDuplicado_DevuelveBadRequest()
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));

        await controller.Register(new MobileRegisterRequest("Ana", "duplicado@test.com", "password123", "paciente"));
        var result = await controller.Register(new MobileRegisterRequest("Otra Ana", "duplicado@test.com", "password123", "paciente"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveAccessTokenYUsuario()
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));
        await controller.Register(new MobileRegisterRequest("Ana", "login@test.com", "password123", "paciente"));

        var result = await controller.Login(new MobileLoginRequest("login@test.com", "password123"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<MobileAuthResponse>(ok.Value);
        Assert.Equal("login@test.com", body.Usuario.Email);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_DevuelveUnauthorized()
    {
        await using var context = TestHarness.NewContext();
        var controller = new MobileAuthController(TestHarness.BuildCommandService(context));
        await controller.Register(new MobileRegisterRequest("Ana", "wrongpass@test.com", "password123", "paciente"));

        var result = await controller.Login(new MobileLoginRequest("wrongpass@test.com", "incorrecta"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
