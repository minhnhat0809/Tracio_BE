using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetRepliesByCommentQuery(int userRequestId, int? replyId, int commentId, int pageNumber, int pageSize) : IRequest<ResponseDto>
{
    public int UserRequestId { get; set; } = userRequestId;
    public int? ReplyId { get; set; } = replyId;
    
    public int? CommentId { get; set; } = commentId;
    
    public int PageNumber { get; set; } = pageNumber;
    
    public int PageSize { get; set; } = pageSize;
}