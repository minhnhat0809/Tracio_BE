using System.Web;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UserService.Application.Interfaces;
using User = UserService.Domain.Entities.User;

namespace UserService.Infrastructure.Repositories;

public class FirebaseStorageRepository : IFirebaseStorageRepository
{
    private const string BucketName = "tracio-cbd26.firebasestorage.app"; // Firebase Storage bucket name
    private readonly StorageClient _storageClient;

    public FirebaseStorageRepository()
    {
        // Initialize Google Cloud Storage client with the service account
        _storageClient = StorageClient.Create(
            GoogleCredential.FromFile("tracio-firebase-adminsdk.json")
        );
    }

    /// <summary>
    /// Uploads multiple image files and returns their public URLs.
    /// </summary>
    public async Task<List<string>> UploadImagesAsync(string name, List<IFormFile> files, string imgFolderName)
    {
        if (files == null || files.Count == 0)
            throw new ArgumentException("No files provided for upload.");

        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
            var uniqueName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{Guid.NewGuid()}";
            var url = await UploadImageAsync(uniqueName, file, imgFolderName);
            uploadedUrls.Add(url);
        }

        return uploadedUrls;
    }

    /// <summary>
    /// Uploads a single image file and returns its public URL.
    /// </summary>
    public async Task<string> UploadImageAsync(string name, IFormFile file, string imgFolderName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty.");

        // Validate ContentType
        var allowedContentTypes = new List<string> { "image/jpeg", "image/png", "image/jpg", "image/gif" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLower()))
        {
            throw new ArgumentException("Only image files (jpeg, jpg, png, gif) are accepted.");
        }

        // Prepare file name and path
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"{imgFolderName}/{name}{fileExtension}";

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        // Create a token for public access
        var token = Guid.NewGuid().ToString();

        // Metadata for file
        var metadata = new Google.Apis.Storage.v1.Data.Object
        {
            Bucket = BucketName,
            Name = fileName,
            ContentType = file.ContentType,
            Metadata = new Dictionary<string, string>
            {
                { "firebaseStorageDownloadTokens", token }
            }
        };

        // Upload file with PublicRead permission
        await _storageClient.UploadObjectAsync(metadata, stream, new UploadObjectOptions
        {
            PredefinedAcl = PredefinedObjectAcl.PublicRead
        });

        // Generate public URL
        return GetFirebaseMediaUrl(fileName, token);
    }

    /// <summary>
    /// Constructs the public Firebase Storage URL for the uploaded file.
    /// </summary>
    private string GetFirebaseMediaUrl(string fileName, string token)
    {
        return $"https://firebasestorage.googleapis.com/v0/b/{BucketName}/o/{Uri.EscapeDataString(fileName)}?alt=media&token={token}";
    }

    /// <summary>
    /// Deletes a file from Firebase Storage by its public URL.
    /// </summary>
    public async Task<bool> DeleteImageByUrlAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);

            // Extract object name from the URL
            string objectName = HttpUtility.UrlDecode(uri.AbsolutePath.Replace($"/v0/b/{BucketName}/o/", ""));

            if (string.IsNullOrEmpty(objectName))
                return false;

            // Delete the object from Firebase Storage
            await _storageClient.DeleteObjectAsync(BucketName, objectName);
            return true;
        }
        catch (Google.GoogleApiException e) when (e.Error.Code == 404)
        {
            Console.WriteLine($"File not found: {e.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting the image: {ex.Message}");
            return false;
        }
    }
}