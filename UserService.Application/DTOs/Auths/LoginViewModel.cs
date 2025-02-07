using UserService.Application.DTOs.Sessions;

namespace UserService.Application.DTOs.Auths;

public class LoginViewModel
{
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

// Session data
    public SessionViewModel Session { get; set; } = null!;
}