using System.Text;
using MediTrack.IdentityService.API.IAM.Domain.Model.Aggregates;
using MediTrack.IdentityService.API.IAM.Domain.Model.ValueObjects;
using MediTrack.IdentityService.API.IAM.Infrastructure.Persistence.EFC.Configuration;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Controllers;
using MediTrack.IdentityService.API.IAM.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.IdentityService.Tests;

/// Adaptación conceptual de ProfilePhotoTests (repositorio origen) a la
/// arquitectura DDD/CQRS del destino: AppDbContext, User del destino,
/// MobileProfileController y FakeProfilePhotoStorage en memoria.
public class MobileProfilePhotoTests
{
    private static MobileProfileController BuildController(AppDbContext context, int userId, FakeProfilePhotoStorage storage)
    {
        return new MobileProfileController(
            TestHarness.BuildQueryService(context),
            TestHarness.BuildCommandService(context),
            storage)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = TestHarness.BuildPrincipal(userId) }
            }
        };
    }

    private static FormFile BuildPhoto(string contentType, string fileName, int sizeBytes = 10)
    {
        var bytes = Encoding.UTF8.GetBytes(new string('a', sizeBytes));
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, stream.Length, "photo", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }

    [Fact]
    public async Task UploadPhoto_ConImagenValida_GuardaYActualizaProfilePhotoUrl()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("ana@test.com", "hash", "Ana", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        var result = await controller.UploadPhoto(BuildPhoto("image/jpeg", "foto.jpg"));

        var ok = Assert.IsType<OkObjectResult>(result);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.NotNull(resource.ProfilePhotoUrl);
        Assert.Single(storage.Files);
        Assert.EndsWith(".jpg", resource.ProfilePhotoUrl);
    }

    [Fact]
    public async Task UploadPhoto_Reemplazo_BorraElArchivoAnteriorSoloDespuesDeConfirmarEnBD()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("luis@test.com", "hash", "Luis", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        await controller.UploadPhoto(BuildPhoto("image/png", "primera.png"));
        var firstFileName = (await context.Users.FindAsync(user.Id))!.ProfilePhotoUrl!;

        await controller.UploadPhoto(BuildPhoto("image/jpeg", "segunda.jpg"));
        var secondFileName = (await context.Users.FindAsync(user.Id))!.ProfilePhotoUrl!;

        Assert.NotEqual(firstFileName, secondFileName);
        Assert.Contains(firstFileName, storage.DeletedFileNames);
        Assert.Single(storage.Files);
    }

    [Theory]
    [InlineData("application/pdf", "documento.pdf")]
    [InlineData("image/jpeg", "sin_extension")]
    public async Task UploadPhoto_ConExtensionNoPermitida_DevuelveBadRequestYNoGuardaNada(string contentType, string fileName)
    {
        await using var context = TestHarness.NewContext();
        var user = new User("eva@test.com", "hash", "Eva", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        var result = await controller.UploadPhoto(BuildPhoto(contentType, fileName));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(storage.Files);
        Assert.Null((await context.Users.FindAsync(user.Id))!.ProfilePhotoUrl);
    }

    [Fact]
    public async Task UploadPhoto_ConContentTypeNoPermitido_DevuelveBadRequestYNoGuardaNada()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("rex@test.com", "hash", "Rex", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        var result = await controller.UploadPhoto(BuildPhoto("application/octet-stream", "foto.jpg"));

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(storage.Files);
    }

    [Fact]
    public async Task UploadPhoto_ExcedeTamanoMaximo_DevuelveBadRequestYNoGuardaNada()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("rob@test.com", "hash", "Rob", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        var oversized = BuildPhoto("image/jpeg", "grande.jpg", sizeBytes: 6 * 1024 * 1024);

        var result = await controller.UploadPhoto(oversized);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Empty(storage.Files);
    }

    [Fact]
    public async Task DeletePhoto_ConFotoExistente_LaBorraYLimpiaProfilePhotoUrl()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("mia@test.com", "hash", "Mia", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        await controller.UploadPhoto(BuildPhoto("image/jpeg", "foto.jpg"));
        var deleteResult = await controller.DeletePhoto();

        var ok = Assert.IsType<OkObjectResult>(deleteResult);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.Null(resource.ProfilePhotoUrl);
        Assert.Empty(storage.Files);
    }

    [Fact]
    public async Task DeletePhoto_SinFotoPrevia_EsIdempotenteYNoFalla()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("tom2@test.com", "hash", "Tom2", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id, new FakeProfilePhotoStorage());

        var result = await controller.DeletePhoto();

        var ok = Assert.IsType<OkObjectResult>(result);
        var resource = Assert.IsType<MobileUserResource>(ok.Value);
        Assert.Null(resource.ProfilePhotoUrl);
    }

    [Fact]
    public async Task GetPhoto_SinFotoGuardada_DevuelveNotFound()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("tom@test.com", "hash", "Tom", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var controller = BuildController(context, user.Id, new FakeProfilePhotoStorage());

        var result = await controller.GetPhoto();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetPhoto_ConFotoGuardada_DevuelveElArchivoConSuContentType()
    {
        await using var context = TestHarness.NewContext();
        var user = new User("zoe@test.com", "hash", "Zoe", UserRole.Patient);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var storage = new FakeProfilePhotoStorage();
        var controller = BuildController(context, user.Id, storage);

        await controller.UploadPhoto(BuildPhoto("image/png", "foto.png"));

        var result = await controller.GetPhoto();

        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/png", fileResult.ContentType);
    }
}
