using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetReactionsByBlogIdQuery : IRequest<ResponseDto>
{
    public int BlogId { get; set; }

    public sbyte ReactionType { get; set; }
}