using Microsoft.AspNetCore.Http;

namespace ContentService.Application.Interfaces;

public interface IImageService
{
    Task<bool> CreateBucket(string bucketName);

    Task<List<string>> GetAllBuckets();

    Task DeleteBucket(string bucketName);

    Task<List<string>> UploadFiles(List<IFormFile> mediaFiles, string bucketName, string? prefix);

    Task DeleteFile(string bucketName, string key);
}