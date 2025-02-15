using Grpc.Core;
using Userservice;
using UserService.Application.Interfaces;

namespace UserService.Api.Services;

public class UserServiceImpl(IUserRepository userRepo) : Userservice.UserService.UserServiceBase
{
    public override async Task<UserResponse> ValidateUser(UserRequest request, ServerCallContext context)
    {
        try
        {
            var user = await userRepo.GetById(u => u.UserId == request.UserId, u => new {u.UserName, u.ProfilePicture});
            if (user != null)
                return new UserResponse
                {
                    IsValid = true,
                    UserName = user.UserName,
                    Avatar = user.ProfilePicture
                };

            return new UserResponse
            {
                IsValid = false,
                UserName = "",
                Avatar = ""
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Error: {ex.Message}"));
        }
    }

    public override async Task<CheckIsFollowResponse> CheckIsFollow(CheckIsFollowRequest request,
        ServerCallContext context)
    {
        try
        {
            var result = await userRepo.AreBothUsersExistAsync(request.UserId1, request.UserId2);
            
            return result ? new CheckIsFollowResponse {IsFollower = true} : new CheckIsFollowResponse {IsFollower = false};
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Error: {ex.Message}"));
        }
    }

    public override async Task<FollowBatchResponse> CheckFollowingBatch(FollowBatchRequest request,
        ServerCallContext context)
    {
        try
        {
            var result = await userRepo.GetFollowingsOfUser(request.FollowerId, request.AuthorIds.ToList());

            var response = new FollowBatchResponse();

            response.FollowingAuthors.AddRange(result);

            return response;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Error: {ex.Message}"));
        }
    }
}