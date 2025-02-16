using ContentService.Application.DTOs.ReactionDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateReactionCommand(ReactionCreateDto reactionCreateDto) : IRequest<ResponseDto>
{
    public int CyclistId { get;} = reactionCreateDto.CyclistId;

    public int EntityId { get; } = reactionCreateDto.EntityId;

    public string EntityType { get; } = reactionCreateDto.EntityType;

}