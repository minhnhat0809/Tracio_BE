using System.ComponentModel.DataAnnotations;

namespace RouteService.Application.DTOs.Coordinates;

public class CoordinateModel
{
    [Required(ErrorMessage = "Latitude is required.")]
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Altitude is required.")]
    [Range(-500, 9000, ErrorMessage = "Altitude must be between -500 and 9000 meters.")]
    public double Altitude { get; set; }
}
    

