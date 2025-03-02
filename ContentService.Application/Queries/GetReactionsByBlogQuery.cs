using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public record GetReactionsByBlogQuery(int BlogId, int PageNumber, int PageSize) : IRequest<ResponseDto>;