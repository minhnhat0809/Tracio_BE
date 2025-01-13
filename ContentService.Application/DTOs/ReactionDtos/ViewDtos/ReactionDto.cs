namespace ContentService.Application.DTOs.ReactionDtos.ViewDtos;

public class ReactionDto
{
    public int UserId { get; set; }
    
    public string UserName { get; set; } = null!;
    
    public DateTime? CreatedAt { get; set; }
}