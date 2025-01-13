using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public record DeleteReplyCommand (int ReplyId) : IRequest<ResponseDto>;