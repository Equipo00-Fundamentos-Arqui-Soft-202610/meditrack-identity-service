using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;
using Microsoft.Extensions.Options;

namespace MediTrack.IdentityService.API.IAM.Infrastructure.Storage;

/// <summary>
/// Implementación de disco local para <see cref="IProfilePhotoStorage"/>. La carpeta
/// es privada (no se registra UseStaticFiles sobre ella); el único acceso es a través
/// de GET /api/v1/profile/photo, resuelto por el usuario dueño del JWT.
/// </summary>
public class LocalProfilePhotoStorage : IProfilePhotoStorage
{
    private readonly string _folderPath;
    private readonly ILogger<LocalProfilePhotoStorage> _logger;

    public LocalProfilePhotoStorage(IOptions<ProfilePhotoStorageOptions> options, ILogger<LocalProfilePhotoStorage> logger)
    {
        _logger = logger;

        var configuredPath = options.Value.StoragePath;
        _folderPath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, "ProfilePhotos")
            : configuredPath;

        Directory.CreateDirectory(_folderPath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(_folderPath, fileName);

        await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(output, cancellationToken);

        return fileName;
    }

    public Stream? OpenRead(string fileName)
    {
        var fullPath = ResolveSafePath(fileName);
        return fullPath != null && File.Exists(fullPath) ? File.OpenRead(fullPath) : null;
    }

    public string GetContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg",
    };

    public void Delete(string fileName)
    {
        try
        {
            var fullPath = ResolveSafePath(fileName);
            if (fullPath != null && File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar la foto de perfil {FileName}", fileName);
        }
    }

    /// El valor guardado en `users.ProfilePhotoUrl` siempre es un nombre de
    /// archivo generado por SaveAsync (GUID + extensión). Igual se valida
    /// aquí para blindar contra path traversal si llegara un valor corrupto.
    private string? ResolveSafePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Contains("..") ||
            fileName.Contains('/') ||
            fileName.Contains('\\'))
        {
            return null;
        }

        return Path.Combine(_folderPath, fileName);
    }
}
