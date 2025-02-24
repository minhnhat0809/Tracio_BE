using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs.Auths;

public class RefreshTokenRequestModel
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
