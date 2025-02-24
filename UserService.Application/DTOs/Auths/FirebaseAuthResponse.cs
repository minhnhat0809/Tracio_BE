namespace UserService.Application.DTOs.Auths;

public class FirebaseAuthResponse
{
    public string IdToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public string ExpiresIn { get; set; } = null!;
}
