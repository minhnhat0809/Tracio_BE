using ContentService.Application.DTOs.UserDtos.View;
using ContentService.Application.Interfaces;
using Google.Protobuf.Collections;
using Userservice;

namespace ContentService.Api.Services;

public class UserService(Userservice.UserService.UserServiceClient userServiceClient) : IUserService
{
    public async Task<UserDto> ValidateUser(int creatorId)
    {
        var request = new UserRequest { UserId = creatorId };
        var response = await userServiceClient.ValidateUserAsync(request);
        
        return new UserDto{IsUserValid = response.IsValid, Username = response.UserName, Avatar = response.Avatar};
    }

    public async Task<UserFollowerDto> CheckUserFollower(int browsingUserId, int userId)
    {
        var request = new UserFollowerRequest { UserId = userId, BrowsingUserId = browsingUserId };
        var response = await userServiceClient.CheckUserAndFollowerAsync(request);

        return new UserFollowerDto{IsExisted = response.IsExisted, IsFollower = response.IsFollower};
    }

    public async Task<List<int>> GetFollowingUserIds(int userId)
    {
        var request = new GetFollowersRequest { UserId = userId };
        
        var response = await userServiceClient.GetFollowerIdsAsync(request);

        return response.FollowerIds.ToList();
    }
}