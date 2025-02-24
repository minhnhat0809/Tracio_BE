namespace UserService.Application.DTOs.Auths;

using System.Text.Json.Serialization;

public class FirebaseSendOtpResponse
{
    [JsonPropertyName("sessionInfo")] // Explicitly map JSON key to C# property
    public string SessionInfo { get; set; } = null!;
}
