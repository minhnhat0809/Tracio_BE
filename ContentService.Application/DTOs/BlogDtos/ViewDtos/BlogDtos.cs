namespace ContentService.Application.DTOs.BlogDtos.ViewDtos;

public class BlogDtos
{
    public int BlogId { get; set; }
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;
    
    public string Avatar { get; set; } = null!;
    
    public sbyte PrivacySetting { get; set; }

    public string Tittle { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? LikesCount { get; set; }

    public int? CommentsCount { get; set; }
}