using ContentService.Application.DTOs.CommentDtos.ViewDtos;

namespace ContentService.Application.DTOs.BlogDtos.ViewDtos;

public class BlogWithCommentsDto
{
    public int BlogId { get; set; }
    
    public int UserId { get; set; }

    public int UserName { get; set; }

    public string Tittle { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;
    
    public List<CommentDto> Comments { get; set; } = null!;
}