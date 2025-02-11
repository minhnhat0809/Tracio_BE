namespace ContentService.Domain.Events;

public class ReactionToCommentEvent(int commentId)
{
    public int CommentId { get; set; } = commentId;
}