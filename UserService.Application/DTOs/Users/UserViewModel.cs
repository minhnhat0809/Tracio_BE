namespace UserService.Application.DTOs.Users;

public class UserViewModel
{
    public int UserId { get; set; }

    public string? UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirebaseId { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? ProfilePicture { get; set; }

    public string? Bio { get; set; }

    public byte[] Role { get; set; } = null!;

    public float? Weight { get; set; }

    public float? Height { get; set; }

    public sbyte Gender { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }
    
}