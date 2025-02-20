namespace ContentService.Application.DTOs.BlogDtos.Message;

public class BlogPrivacyUpdateEvent (int blogId, List<int> followerIds, string action)
{
    public int BlogId { get; set; } = blogId;

    public List<int> FollowerIds { get; set; } = followerIds;

    public string Action { get; set; } = action;
}