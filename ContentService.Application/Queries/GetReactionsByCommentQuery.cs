using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public record GetReactionsByCommentQuery(int CommentId, int PageNumber, int PageSize) : IRequest<ResponseDto>;