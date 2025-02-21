using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetBlogsQuery(int userRequestId, int? userId, int? categoryId, string sortBy, bool ascending, int pageSize, int pageNumber) : IRequest<ResponseDto>
{
    public int UserRequestId { get; set; } = userRequestId;

    public int? UserId { get; set; } = userId;

    public int? CategoryId { get; set; } = categoryId;

    public string SortBy { get; set; } = sortBy;

    public bool Ascending { get; set; } = ascending;

    public int PageSize { get; set; } = pageSize;

    public int PageNumber { get; set; } = pageNumber;
}