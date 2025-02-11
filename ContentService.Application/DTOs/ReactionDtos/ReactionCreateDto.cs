namespace ContentService.Application.DTOs.ReactionDtos;

public class ReactionCreateDto
{
    public int CyclistId { get; set; }
    
    public string CyclistName { get; set; } = null!;
    
    public int EntityId { get; set; }
    
    public string EntityType { get; set; } = null!;
}