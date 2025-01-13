using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public record DeleteCommentCommand (int CommentId) : IRequest<ResponseDto>;