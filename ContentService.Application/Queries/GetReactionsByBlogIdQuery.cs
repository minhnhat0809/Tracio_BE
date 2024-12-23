using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetReactionsByBlogIdQuery : IRequest<ResponseDto>
{
    public string BlogId { get; set; } = null!;
    
    public sbyte ReactionType { get; set; }
}