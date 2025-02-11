using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs.Users;

public class UpdateUserProfileModel
{
    public string? DisplayName { get; set; }
    
    public string? Bio { get; set; }
    
    public float? Weight { get; set; }

    public float? Height { get; set; }

    public sbyte Gender { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }
    
    public IFormFile? AvatarFile { get; set; }
}