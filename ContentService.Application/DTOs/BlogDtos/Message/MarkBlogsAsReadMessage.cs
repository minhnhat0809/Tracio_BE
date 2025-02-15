namespace ContentService.Application.DTOs.BlogDtos.Message;

public class MarkBlogsAsReadMessage
{
    public int UserId { get; set; }
    
    public List<int> BlogIds { get; set; } = [];
}