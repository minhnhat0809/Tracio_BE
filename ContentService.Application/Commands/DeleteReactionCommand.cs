using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public record DeleteReactionCommand(int UserRequestId, int EntityId, string EntityType) : IRequest<ResponseDto>;