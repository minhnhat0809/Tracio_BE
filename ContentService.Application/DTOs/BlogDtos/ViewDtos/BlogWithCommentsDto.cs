using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;

namespace ContentService.Application.DTOs.BlogDtos.ViewDtos;

public class BlogWithCommentsDto
{
    public int BlogId { get; set; }
    
    public int CreatorId { get; set; }

    public string CreatorName { get; set; } = null!;

    public string CreatorAvatar { get; set; } = null!;
    
    public string CategoryName { get; set; } = null!;

    public string Content { get; set; } = null!;
    
    public bool IsReacted { get; set; }
    
    public bool IsBookmarked { get; set; }
    
    public List<MediaFileDto> MediaFiles { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;
}