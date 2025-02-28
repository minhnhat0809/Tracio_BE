using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public record DeleteBookmarkCommand(int BlogId, int UserRequestId) : IRequest<ResponseDto>;