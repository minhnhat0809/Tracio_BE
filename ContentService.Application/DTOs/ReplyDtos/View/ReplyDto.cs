using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;

namespace ContentService.Application.DTOs.ReplyDtos.View;

public class ReplyDto
{
    public int ReplyId { get; set; }

    public int CyclistId { get; set; }

    public int CommentId { get; set; }

    public string CyclistName { get; set; } = null!;

    public string Content { get; set; } = null!;
    
    public bool IsReacted { get; set; }
    
    public List<MediaFileDto> MediaFiles { get; set; } = [];

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? LikesCount { get; set; }

    public bool? IsEdited { get; set; }
}