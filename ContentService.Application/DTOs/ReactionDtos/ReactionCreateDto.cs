namespace ContentService.Application.DTOs.ReactionDtos;

public class ReactionCreateDto
{
    public int EntityId { get; set; }
    
    public string EntityType { get; set; } = null!;
}