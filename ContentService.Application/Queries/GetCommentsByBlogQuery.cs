using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetCommentsByBlogQuery(int blogId, int? commentId, int pageNumber, int pageSize, bool isAscending) : IRequest<ResponseDto>
{
    public int BlogId { get; set; } = blogId;
    
    public int? CommentId { get; set; } = commentId;
    
    public int PageNumber { get; set; } = pageNumber;
    
    public int PageSize { get; set; } = pageSize;
    
    public bool IsAscending { get; set; } = isAscending;
}