using MediatR;
using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

public class GetFollowerIdsQueryHandler(IFollowerRepo followerRepo) : IRequestHandler<GetFollowerIdsQuery, List<int>>
{
    private readonly IFollowerRepo _followerRepo = followerRepo;
    
    public async Task<List<int>> Handle(GetFollowerIdsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var followerIds = await _followerRepo.FindAsync(f => f.FollowedId == request.UserId, f => f.FollowerId);

            return followerIds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}