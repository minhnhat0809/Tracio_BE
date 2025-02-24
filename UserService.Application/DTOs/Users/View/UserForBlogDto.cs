namespace UserService.Application.DTOs.Users.View;

public class UserForBlogDto(string userName, string avatar)
{
    public string Username { get; set; } = userName;

    public string Avatar { get; set; } = avatar;
}