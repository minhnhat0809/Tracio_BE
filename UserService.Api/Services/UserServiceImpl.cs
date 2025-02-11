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
            var user = await userRepo.GetById(u => u.UserId == request.UserId, u => u.UserName);
            if (user != null)
                return new UserResponse
                {
                    IsValid = true,
                    UserName = user
                };

            return new UserResponse
            {
                IsValid = false,
                UserName = ""
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, $"Internal Error: {ex.Message}"));
        }
    }
}