
namespace ContentService.Application.DTOs.CommentDtos.ViewDtos;

public class CommentDto
{
    public string CommentId { get; set; } = null!;

    public string UserId { get; set; } = null!;
    
    public string UserName { get; set; } = null!;
    
    public string Content { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsEdited { get; set; }
    
    public int LikesCount { get; set; }
}