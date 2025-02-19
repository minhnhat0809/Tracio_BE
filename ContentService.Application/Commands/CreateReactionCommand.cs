using ContentService.Application.DTOs.ReactionDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateReactionCommand(int cyclistId, ReactionCreateDto reactionCreateDto) : IRequest<ResponseDto>
{
    public int CyclistId { get;} = cyclistId;

    public int EntityId { get; } = reactionCreateDto.EntityId;

    public string EntityType { get; } = reactionCreateDto.EntityType;

}