namespace ContentService.Application.DTOs.UserDtos.View;

public class UserDto
{
    public bool IsUserValid { get; set; }

    public string Username { get; set; } = null!;
    
    public string Avatar { get; set; } = null!;
}