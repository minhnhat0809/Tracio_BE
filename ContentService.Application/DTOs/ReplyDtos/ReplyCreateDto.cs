namespace ContentService.Application.DTOs.ReplyDtos;

public class ReplyCreateDto
{
    public int CommentId { get; set; }
    
    public string Content { get; set; } = null!;
}