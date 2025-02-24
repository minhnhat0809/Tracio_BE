using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs.Users;

public class ShopOwnerRegisterModel
{
    [Required(ErrorMessage = "Firebase UID is required.")]
    public string FirebaseUid { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [StringLength(100, ErrorMessage = "UserName cannot exceed 100 characters.")]
    public required string UserName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [StringLength(50, ErrorMessage = "Provider ID cannot exceed 50 characters.")]
    public string? ProviderId { get; set; }

    public IFormFile? AvatarFile { get; set; }

    // Shop Information
    [StringLength(20, ErrorMessage = "Tax Code cannot exceed 20 characters.")]
    public string? TaxCode { get; set; }

    [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
    public string? Address { get; set; }
}