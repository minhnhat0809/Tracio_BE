using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateReactionCommand : IRequest<ResponseDto>
{
    public int CyclistId { get; set; }
    
    public string CyclistName { get; set; } = null!;
    
    public int EntityId { get; set; }
    
    public string EntityType { get; set; } = null!;
    
    public sbyte ReactionType { get; set; }
    
}