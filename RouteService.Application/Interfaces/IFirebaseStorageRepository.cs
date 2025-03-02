using Microsoft.AspNetCore.Http;

namespace RouteService.Application.Interfaces;

public interface IFirebaseStorageRepository
{
    Task<List<string>> UploadImagesAsync(string name, List<IFormFile> files, string imgFolderName/*,
        CancellationToken cancellationToken*/);

    Task<string> UploadImageAsync(string name, IFormFile file, string imgFolderName/*,
        CancellationToken cancellationToken*/);

    Task<bool> DeleteImageByUrlAsync(string fileUrl/*, CancellationToken cancellationToken*/);
}