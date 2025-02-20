using MediatR;
using UserService.Application.DTOs.Users.Grpc;

namespace UserService.Application.Queries;

public class CheckIsFollowQuery(int userId, int userRequestId) : IRequest<IsFollowerResponse>
{
    public int UserId { get; set; } = userId;
    
    public int UserRequestId { get; set; } = userRequestId;
}