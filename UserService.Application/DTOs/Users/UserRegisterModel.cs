using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs.Users;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class UserRegisterModel
{
    [Required(ErrorMessage = "FirebaseUid is required.")]
    public string FirebaseUid { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Display Name cannot exceed 100 characters.")]
    public string? UserName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Only JPG, JPEG, or PNG files are allowed.")]
    public IFormFile? AvatarFile { get; set; }
}
