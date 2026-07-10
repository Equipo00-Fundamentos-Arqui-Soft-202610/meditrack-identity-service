using MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;

namespace MediTrack.IdentityService.Tests;

/// Doble en memoria de IProfilePhotoStorage: evita tocar disco real en los
/// tests y permite verificar qué se guardó/borró.
public class FakeProfilePhotoStorage : IProfilePhotoStorage
{
    public readonly Dictionary<string, byte[]> Files = new();
    public readonly List<string> DeletedFileNames = new();

    public Task<string> SaveAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default)
    {
        using var memory = new MemoryStream();
        fileStream.CopyTo(memory);

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        Files[fileName] = memory.ToArray();
        return Task.FromResult(fileName);
    }

    public Stream? OpenRead(string fileName) =>
        Files.TryGetValue(fileName, out var bytes) ? new MemoryStream(bytes) : null;

    public string GetContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg",
    };

    public void Delete(string fileName)
    {
        Files.Remove(fileName);
        DeletedFileNames.Add(fileName);
    }
}
