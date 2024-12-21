using ContentService.Application.DTOs;
using MediatR;

namespace ContentService.Application.Queries;

public class GetBlogsQuery : IRequest<ResponseDto>
{
    public string? UserId { get; set; }
    
    public sbyte? Status { get; set; }
    
    public string? SortBy { get; set; }

    public bool Ascending { get; set; }

    public int PageSize { get; set; }

    public int PageNumber { get; set; }
}