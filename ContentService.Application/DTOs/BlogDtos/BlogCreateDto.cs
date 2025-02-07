using Microsoft.AspNetCore.Http;


namespace ContentService.Application.DTOs.BlogDtos;

public class BlogCreateDto
{
    public int CreatorId { get; set; }
    
    public int CategoryId { get; set; }
    
    public IFormFileCollection? MediaFiles { get; set; }
    
    public string Content { get; set; } = null!;
    
    public string PrivacySetting { get; set; } = null!;
    
    public sbyte Status { get; set; }
}