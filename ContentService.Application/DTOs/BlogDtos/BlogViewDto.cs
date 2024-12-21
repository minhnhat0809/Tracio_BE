namespace ContentService.Application.DTOs.BlogDtos;

public class BlogViewDto
{
    public string BlogId { get; set; } = null!;
        
    public string UserId { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Tittle { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int LikesCount { get; set; }

    public int CommentsCount { get; set; }
}