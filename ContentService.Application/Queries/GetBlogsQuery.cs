using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetBlogsQuery : IRequest<ResponseDto>
{
    public int? UserId { get; set; }
    
    public sbyte? Status { get; set; }
    
    public string? SortBy { get; set; }

    public bool Ascending { get; set; }

    public int PageSize { get; set; }

    public int PageNumber { get; set; }
}