using MediatR;

namespace UserService.Application.Queries;

public class GetFollowerIdsQuery(int userId) : IRequest<List<int>>
{
    public int UserId { get; set; } = userId;
}