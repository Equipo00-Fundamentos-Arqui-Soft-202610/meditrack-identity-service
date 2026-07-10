namespace MediTrack.IdentityService.API.IAM.Application.Internal.OutboundServices;

public interface IProfilePhotoStorage
{
    Task<string> SaveAsync(Stream fileStream, string fileExtension, CancellationToken cancellationToken = default);

    Stream? OpenRead(string fileName);

    string GetContentType(string fileName);

    void Delete(string fileName);
}
