using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public class GetBookmarksQuery(int ownerId, int pageSize, int pageNumber) : IRequest<ResponseDto>
{
    public int OwnerId { get; set; } = ownerId;

    public int PageSize { get; set; } = pageSize;

    public int PageNumber { get; set; } = pageNumber;
}