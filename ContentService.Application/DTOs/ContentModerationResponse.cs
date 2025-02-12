namespace ContentService.Application.DTOs;

public class ContentModerationResponse
{
    public bool IsSafe { get; set; }
    public List<string> FlaggedCategories { get; set; } = new();
}