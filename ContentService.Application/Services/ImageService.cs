using System.Collections.Concurrent;
using Amazon.S3;
using Amazon.S3.Model;
using ContentService.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ContentService.Application.Services;

public class ImageService (IAmazonS3 s3Client) : IImageService
{
    private readonly IAmazonS3 _s3Client = s3Client;
    
    public async Task<bool> CreateBucket(string bucketName)
    {
        var bucketExist = await Amazon.S3.Util.AmazonS3Util.
            DoesS3BucketExistV2Async(_s3Client, bucketName);

        if (bucketExist) return false;

        await _s3Client.PutBucketAsync(bucketName);
        return true;
    }

    public async Task<List<string>> GetAllBuckets()
    {
        var data = await _s3Client.ListBucketsAsync();

        var bucketList = data.Buckets.Select(b => b.BucketName.ToString()).ToList();

        return bucketList;
    }

    public async Task DeleteBucket(string bucketName)
    {
        await _s3Client.DeleteBucketAsync(bucketName);
    }

    public async Task<List<string>> UploadFiles(List<IFormFile> files, string bucketName, string? prefix = null)
    {
        var uploadTasks = new ConcurrentBag<Task<string>>();

        foreach (var file in files)
        {
            uploadTasks.Add(UploadFileAsync(file, bucketName, prefix));
        }

        var results = await Task.WhenAll(uploadTasks);
        return results.ToList();
    }
    private async Task<string> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
    {
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var s3Key = string.IsNullOrEmpty(prefix) ? fileName : $"{prefix.TrimEnd('/')}/{fileName}";

        try
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());

            // Define the desired width while maintaining aspect ratio
            const int desiredWidth = 800;
            var newHeight = (int)(image.Height * (desiredWidth / (double)image.Width));

            image.Mutate(x => x.Resize(desiredWidth, newHeight));

            using var memoryStream = new MemoryStream();
            await image.SaveAsJpegAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Key,
                InputStream = memoryStream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{bucketName}.s3.amazonaws.com/{s3Key}";
        }
        catch (AmazonS3Exception ex)
        {
            // Log exception
            throw new Exception($"Error uploading file: {ex.Message}", ex);
        }
    }

    public async Task DeleteFile(string bucketName, string key)
    {
        try
        {
            // Check if the bucket exists
            var bucketExist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExist) throw new Exception("There are no buckets with this name");
            
            //define delete object
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        }
        catch (AmazonS3Exception e)
        {
            throw new AmazonS3Exception(e.Message);
        }
    }
}