using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetReactionsByCommentQuery : IRequest<ResponseDto>
{
    public int CommentId { get; set; }
}