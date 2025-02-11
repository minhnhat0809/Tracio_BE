using ContentService.Application.DTOs.UserDtos.View;
using ContentService.Application.Interfaces;
using Userservice;

namespace ContentService.Api.Services;

public class UserService(Userservice.UserService.UserServiceClient userServiceClient) : IUserService
{
    public async Task<UserDto> ValidateUser(int creatorId)
    {
        var request = new UserRequest { UserId = creatorId };
        var response = await userServiceClient.ValidateUserAsync(request);
        
        return new UserDto{IsUserValid = response.IsValid, Username = response.UserName};
    }
}