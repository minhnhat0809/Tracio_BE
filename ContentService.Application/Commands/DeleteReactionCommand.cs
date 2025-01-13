using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class DeleteReactionCommand(int ReactionId) : IRequest<ResponseDto>;