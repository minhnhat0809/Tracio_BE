
namespace ContentService.Application.DTOs.CommentDtos.ViewDtos;

public class CommentDto
{
    public int CommentId { get; set; }

    public int UserId { get; set; }
    
    public string UserName { get; set; } = null!;
    
    public string Content { get; set; } = null!;
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool? IsEdited { get; set; }
    
    public int? LikesCount { get; set; }
}