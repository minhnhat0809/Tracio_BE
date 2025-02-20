namespace ContentService.Application.DTOs.BlogDtos.Message;

public class BlogPrivacyUpdateEvent (int userId, int blogId, string action)
{
    public int BlogId { get; set; } = blogId;
    
    public int UserId { get; set; } = userId;

    public string Action { get; set; } = action;
}