using Microsoft.AspNetCore.Http;


namespace ContentService.Application.DTOs.BlogDtos;

public class BlogCreateDto
{
    public int CreatorId { get; set; }
    
    public int CategoryId { get; set; }
    
    public string Content { get; set; } = null!;
    
    public sbyte PrivacySetting { get; set; }
    
    public sbyte Status { get; set; }
}