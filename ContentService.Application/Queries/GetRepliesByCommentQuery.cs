using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public record GetRepliesByCommentQuery (int CommentId, int PageNumber = 1, int PageSize = 10) : IRequest<ResponseDto>;