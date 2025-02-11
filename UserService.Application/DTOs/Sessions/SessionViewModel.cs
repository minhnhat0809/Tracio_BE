namespace UserService.Application.DTOs.Sessions;

public class SessionViewModel
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    //public string? IpAddress { get; set; }
    //public string? UserAgent { get; set; }
    public DateTime? ExpiresAt { get; set; }
}