namespace ContentService.Domain.Events;

public class ReactionToBlogEvent (int blogId)
{
    public int BlogId { get; set; } = blogId;
}