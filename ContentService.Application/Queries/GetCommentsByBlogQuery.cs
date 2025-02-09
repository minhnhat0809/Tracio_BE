using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetCommentsByBlogQuery : IRequest<ResponseDto>
{
    public int BlogId { get; set; }
    
    public int PageNumber { get; set; } = 1;
    
    public int PageSize { get; set; } = 5;
    
    public bool IsAscending { get; set; } = true;
}