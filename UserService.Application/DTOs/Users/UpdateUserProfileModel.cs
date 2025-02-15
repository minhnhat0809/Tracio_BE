using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs.Users;

public class UpdateUserProfileModel
{
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public string? UserName { get; set; }

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
    public string? Bio { get; set; }

    [Range(0, 500, ErrorMessage = "Weight must be between 0 and 500 kg.")]
    public float? Weight { get; set; }

    [Range(0, 300, ErrorMessage = "Height must be between 0 and 300 cm.")]
    public float? Height { get; set; }

    [Range(0, 2, ErrorMessage = "Gender must be 0 (unknown), 1 (male), or 2 (female).")]
    public sbyte Gender { get; set; }

    [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "District name cannot exceed 100 characters.")]
    public string? District { get; set; }

    //[Phone(ErrorMessage = "Invalid phone number format.")]
    //public string? PhoneNumber { get; set; } // ✅ Ensures it's a valid phone number or NULL.

    //[DataType(DataType.Upload)]
    //public IFormFile? AvatarFile { get; set; }
}