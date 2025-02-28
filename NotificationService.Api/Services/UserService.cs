using NotificationService.Application.Interfaces;
using Userservice;

namespace NotificationService.Api.Services;

public class UserService(Userservice.UserService.UserServiceClient userServiceClient) : IUserService
{
    public async Task<bool> CheckUserValid(int userId)
    {
        var request = new IsUserValidRequest { UserId = userId };
        var response = await userServiceClient.IsUserValidAsync(request);

        return response.IsValid;
    }
}