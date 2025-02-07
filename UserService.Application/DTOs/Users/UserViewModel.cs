namespace UserService.Application.DTOs.Users;

public class UserViewModel
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? PhotoUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ProviderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    
    public string? Bio { get; set; }
    
    public float? Weight { get; set; }

    public float? Height { get; set; }

    public sbyte Gender { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }
}