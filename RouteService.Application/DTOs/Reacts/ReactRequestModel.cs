using System.ComponentModel.DataAnnotations;

namespace RouteService.Application.DTOs.Reacts;

public class ReactRequestModel
{
    [Required]
    public int CyclistId { get; set; }

    [Required]
    public int TargetId { get; set; }

    [Required]
    [RegularExpression("^(route|comment)$", ErrorMessage = "TargetType must be 'route' or 'comment'.")]
    public string TargetType { get; set; } = string.Empty;
}