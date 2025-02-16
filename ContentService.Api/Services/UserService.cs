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

    public async Task<bool> IsFollower(int userRequestId, int userId)
    {
        var request = new CheckIsFollowRequest { UserId1 = userRequestId, UserId2 = userId };
        var response = await userServiceClient.CheckIsFollowAsync(request);

        return response.IsFollower;
    }

    public async Task<List<int>> CheckFollowings(int userId, List<int> authorIds)
    {
        var request = new FollowBatchRequest 
        { 
            FollowerId = userId
        };

        request.AuthorIds.AddRange(authorIds); 

        var response = await userServiceClient.CheckFollowingBatchAsync(request);

        return response.FollowingAuthors.ToList(); 
    }

}