namespace ContentService.Application.DTOs.ReactionDtos.ViewDtos;

public class ReactionDto
{
    public string UserId { get; set; } = null!;
    
    public string UserName { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
}