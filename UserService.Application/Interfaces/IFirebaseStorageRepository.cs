using Microsoft.AspNetCore.Http;

namespace UserService.Application.Interfaces;

public interface IFirebaseStorageRepository
{
    Task<List<string>> UploadImagesAsync(string name, List<IFormFile> files, string imgFolderName);
    Task<string> UploadImageAsync(string name, IFormFile file, string imgFolderName);
    Task<bool> DeleteImageByUrlAsync(string fileUrl);
}