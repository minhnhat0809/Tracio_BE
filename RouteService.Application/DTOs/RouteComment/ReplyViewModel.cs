namespace RouteService.Application.DTOs.RouteComment;

public class ReplyViewModel
{
    public int CommentId { get; set; }

    public int RouteId { get; set; }

    public int? ParentCommentId { get; set; }

    public int CyclistId { get; set; }

    public int? MentionCyclistId { get; set; }

    public string CommentContent { get; set; } = null!;

    public int ReactionCounts { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}