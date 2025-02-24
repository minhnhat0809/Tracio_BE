namespace ContentService.Application.DTOs.BlogDtos.Message;

public class MarkBlogsAsReadEvent
{
    public int UserId { get; set; }
    
    public List<int> BlogIds { get; set; } = [];
}