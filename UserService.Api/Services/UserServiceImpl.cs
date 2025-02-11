/*using Grpc.Core;
using Userservice;

namespace UserService.Api.Services;

public class UserServiceImpl(IUserValidationService userValidationService) : UserService.UserServiceBase
{
    private readonly IUserValidationService _userValidationService = userValidationService;

    public override async Task<UserResponse> ValidateUser(UserRequest request, ServerCallContext context)
    {
        var user = await _userValidationService.ValidateUserAsync(request.UserId);
        return new UserResponse
        {
            IsValid = user != null,
            UserName = user?.UserName ?? ""
        };
    }
}*/