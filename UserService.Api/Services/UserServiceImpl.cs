using Grpc.Core;
using MediatR;
using Userservice;
using UserService.Application.Queries;

namespace UserService.Api.Services;

public class UserServiceImpl(IMediator mediator) : Userservice.UserService.UserServiceBase
{
    private readonly IMediator _mediator = mediator;
    
    public override async Task<UserResponse> ValidateUser(UserRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _mediator.Send(new GetUserInformationQuery(request.UserId));
            if (user != null)
                return new UserResponse
                {
                    IsValid = true,
                    UserName = user.Username,
                    Avatar = user.Avatar
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

    public override async Task<UserFollowerResponse> CheckUserAndFollower(UserFollowerRequest request,
        ServerCallContext context)
    {
        try
        {
            var result = await _mediator.Send(new CheckIsFollowQuery(request.UserId, request.BrowsingUserId));

            return new UserFollowerResponse { IsExisted = result.IsExisted, IsFollower = result.IsFollower};
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Error: {ex.Message}"));
        }
    }

    public override async Task<GetFollowersResponse> GetFollowerIds(GetFollowersRequest request, ServerCallContext context)
    {
        try
        {
            var result = await _mediator.Send(new GetFollowerIdsQuery(request.UserId));

            var response = new GetFollowersResponse();
            
            response.FollowerIds.AddRange(result);
            
            return response;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Error: {ex.Message}"));
        }
    }
}