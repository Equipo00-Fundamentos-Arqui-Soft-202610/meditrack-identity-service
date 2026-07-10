namespace MediTrack.IdentityService.API.IAM.Infrastructure.Storage;

public sealed class ProfilePhotoStorageOptions
{
    public const string SectionName = "ProfilePhotos";

    public string? StoragePath { get; set; }
}
