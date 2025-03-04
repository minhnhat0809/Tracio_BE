using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Auths;

public class VerifyPhoneNumberLinkRequestModel
{
    [Required(ErrorMessage = "IdToken is required.")]
    public string IdToken { get; set; } = null!;

    [Required(ErrorMessage = "Verification ID is required.")]
    public string VerificationId { get; set; } = null!;

    [Required(ErrorMessage = "OTP Code is required.")]
    public string OtpCode { get; set; } = null!;

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; } = null!;
}
