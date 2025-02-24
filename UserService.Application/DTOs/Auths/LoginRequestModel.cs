namespace UserService.Application.DTOs.Auths;

public class LoginRequestModel
{
    public string? IdToken { get; set; } // Login with Firebase/Google
    public string? RefreshToken { get; set; } // Login with Firebase/Google
    public string? Email { get; set; } // Login with Email/Password
    public string? Password { get; set; } // Login with Email/Password
    //public string? IpAddress { get; set; } // Client's IP Address
    //public string? UserAgent { get; set; } // Client's User-Agent (Browser/Device)
}