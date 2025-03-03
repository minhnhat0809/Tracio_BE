namespace ContentService.Application.DTOs.ReplyDtos;

public class ReplyCreateDto
{
    public int? ReplyId { get; set; }
    
    public int CommentId { get; set; }
    
    public string Content { get; set; } = null!;
}