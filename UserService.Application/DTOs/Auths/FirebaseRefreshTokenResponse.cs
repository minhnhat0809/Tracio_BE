namespace UserService.Application.DTOs.Auths;

using System.Text.Json.Serialization;


public class FirebaseRefreshTokenResponse
{
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = null!;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("project_id")]
    public string ProjectId { get; set; } = null!;
}
