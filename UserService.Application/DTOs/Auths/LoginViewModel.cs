using UserService.Application.DTOs.Sessions;

namespace UserService.Application.DTOs.Auths;

public class LoginViewModel
{
    public int UserId { get; set; }

    public string? UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirebaseId { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }

// Session data
    public SessionViewModel Session { get; set; } = null!;
}