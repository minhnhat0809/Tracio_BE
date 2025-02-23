using ShopService.Application.DTOs.MediaDtos.ViewDtos;
using ShopService.Domain.Entities;

namespace ShopService.Application.DTOs.ServiceDtos.ViewDtos;

public class ServiceDto
{
    public int ServiceId { get; set; }

    public int ShopId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly ClosedTime { get; set; }

    public sbyte Status { get; set; }

    public TimeOnly Duration { get; set; }
    
    public List<MediaDto> MediaFiles { get; set; } = [];
}