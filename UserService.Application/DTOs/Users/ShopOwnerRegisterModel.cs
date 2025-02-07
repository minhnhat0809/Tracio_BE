using Microsoft.AspNetCore.Http;

namespace UserService.Application.DTOs.Users;

public class ShopOwnerRegisterModel
{
    public string FirebaseUid { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProviderId { get; set; }
    public IFormFile? AvatarFile { get; set; }
    
    // shop info
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
}