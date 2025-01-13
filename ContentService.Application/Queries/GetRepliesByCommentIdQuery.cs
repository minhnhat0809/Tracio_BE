using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public record GetRepliesByCommentIdQuery (int CommentId, int PageNumber = 1, int PageSize = 10) : IRequest<ResponseDto>;