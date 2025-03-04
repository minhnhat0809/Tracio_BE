using MediatR;
using UserService.Application.DTOs.Users.Grpc;
using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

public class CheckIsFollowQueryHandler(IUserRepository userRepository, IFollowerRepo followerRepo) : IRequestHandler<CheckIsFollowQuery, IsFollowerResponse>
{
    private readonly IUserRepository _userRepository = userRepository;
    
    private readonly IFollowerRepo _followerRepo = followerRepo;
    
    public async Task<IsFollowerResponse> Handle(CheckIsFollowQuery request, CancellationToken cancellationToken)
    {
        try
        {

            var isUserExisted = await _userRepository.ExistsAsync(u => u.UserId == request.UserId);

            if (!isUserExisted) return new IsFollowerResponse { IsExisted = false, IsFollower = false };
            
            var isFollower = await _followerRepo.ExistsAsync(f => f.FollowerId == request.UserRequestId && f.FollowedId == request.UserId);
            
            return new IsFollowerResponse{IsExisted = isUserExisted, IsFollower = isFollower};
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}