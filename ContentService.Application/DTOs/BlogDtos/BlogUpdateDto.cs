namespace ContentService.Application.DTOs.BlogDtos;

public class BlogUpdateDto
{
    public string? Content { get; set; }
    
    public sbyte? PrivacySetting { get; set; }
}