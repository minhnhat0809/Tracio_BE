namespace RouteService.Application.DTOs.Users;

public class UserResponse 
{
    public bool IsUserValid { get; set; }

    public string Username { get; set; } = null!;
    
    public string Avatar { get; set; } = null!;
}