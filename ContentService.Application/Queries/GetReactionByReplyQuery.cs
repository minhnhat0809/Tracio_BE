using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries;

public record GetReactionByReplyQuery (int ReplyId) : IRequest<ResponseDto>;