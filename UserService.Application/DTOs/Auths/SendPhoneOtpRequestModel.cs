using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Auths;

public class SendPhoneOtpRequestModel
{
    [Required(ErrorMessage = "Phone Number is required.")]
    public required string? RecaptchaToken { get; set; } 
    
    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format.")]
    public required string? PhoneNumber { get; set; } 
}