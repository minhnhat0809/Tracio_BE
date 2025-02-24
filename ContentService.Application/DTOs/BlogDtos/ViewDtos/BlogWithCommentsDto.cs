using ContentService.Application.DTOs.CommentDtos.ViewDtos;

namespace ContentService.Application.DTOs.BlogDtos.ViewDtos;

public class BlogWithCommentsDto
{
    public int BlogId { get; set; }
    
    public int CreatorId { get; set; }

    public string CreatorName { get; set; } = null!;

    public string CreatorAvatar { get; set; } = null!;
    
    public string CategoryName { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;
    public List<CommentDto> Comments { get; set; } = null!;
}