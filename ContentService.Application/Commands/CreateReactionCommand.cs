using ContentService.Application.DTOs.ReactionDtos;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateReactionCommand(ReactionCreateDto reactionCreateDto) : IRequest<ResponseDto>
{
    public int CyclistId { get;} = reactionCreateDto.CyclistId;

    public string CyclistName { get; } = reactionCreateDto.CyclistName;

    public int EntityId { get; } = reactionCreateDto.EntityId;

    public string EntityType { get; } = reactionCreateDto.EntityType;

}