using ContentService.Application.DTOs.CommentDtos.ViewDtos;

namespace ContentService.Application.DTOs.BlogDtos.ViewDtos;

public class BlogWithCommentsDto
{
    public string BlogId { get; set; } = null!;
    
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Tittle { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;
    
    public List<CommentDto> Comments { get; set; } = null!;
}