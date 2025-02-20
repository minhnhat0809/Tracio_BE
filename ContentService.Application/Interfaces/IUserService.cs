using ContentService.Application.DTOs.UserDtos.View;

namespace ContentService.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> ValidateUser(int creatorId);
    
    Task<bool> IsFollower(int userRequestId, int userId);
}