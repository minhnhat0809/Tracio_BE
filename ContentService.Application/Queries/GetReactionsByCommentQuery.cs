using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetReactionsByCommentQuery(int commentId) : IRequest<ResponseDto>
{
    public int CommentId { get; set; } = commentId;
}