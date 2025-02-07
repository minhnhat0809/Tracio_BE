using ContentService.Domain.Enums;

namespace ContentService.Application.DTOs.ReactionDtos.ViewDtos;

public class ReactionDto
{
    public int ReactionId { get; set; }
    
    public int UserId { get; set; }
    
    public string UserName { get; set; } = null!;
    
    public string ReactionType { get; set; } = null!;
    
    public DateTime? CreatedAt { get; set; }
}