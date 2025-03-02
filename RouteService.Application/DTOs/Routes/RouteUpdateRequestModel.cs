namespace RouteService.Application.DTOs.Routes;

public class RouteUpdateRequestModel
{
    // Route Information
    public string RouteName { get; set; } = null!;

    public string? Description { get; set; }

    public string? City { get; set; }
    
    
    // User Information

    public sbyte PrivacyLevel { get; set; }

    public bool? IsGroup { get; set; }

    public sbyte? Mood { get; set; }

    public sbyte Difficulty { get; set; }

    public bool? IsPublic { get; set; }

}