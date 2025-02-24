using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Auths;

public class VerifyPhoneOtpRequestModel
{
    [Required(ErrorMessage = "Verification ID is required.")]
    public string VerificationId { get; set; } = null!;

    [Required(ErrorMessage = "OTP Code is required.")]
    public string OtpCode { get; set; } = null!;
}
