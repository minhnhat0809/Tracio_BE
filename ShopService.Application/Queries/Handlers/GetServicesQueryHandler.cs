using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;
using ShopService.Application.DTOs.MediaDtos.ViewDtos;
using ShopService.Application.DTOs.ServiceDtos.ViewDtos;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;

namespace ShopService.Application.Queries.Handlers;

public class GetServicesQueryHandler(IServiceRepo serviceRepo) : IRequestHandler<GetServicesQuery, ResponseDto>
{
    private readonly IServiceRepo _serviceRepo = serviceRepo;
    
    public async Task<ResponseDto> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var basePredicate = PredicateBuilder.New<Service>(true);
            
            // filter by category
            basePredicate = basePredicate.And(b => b.CategoryId == request.CategoryId);
            
            // filter by shop
            basePredicate = basePredicate.And(b => b.ShopId == request.ShopId);

            if (request.Status.HasValue)
            {
                if(!IsValidServiceStatus(request.Status.Value)) return ResponseDto.BadRequest("Status is invalid!");
                
                basePredicate = basePredicate.And(b => b.Status == request.Status.Value);
            }

            var serviceDtos = await _serviceRepo.FindAsync(basePredicate, s => new ServiceDto
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                OpenTime = s.OpenTime,
                ClosedTime = s.ClosedTime,
                Status = s.Status,
                Duration = s.Duration,
                MediaFiles = s.MediaFiles.Select(mf => new MediaDto{MediaFileId = mf.MediaFileId, MediaUrl = mf.MediaUrl}).ToList(),
            }, s => s.MediaFiles);
            
            serviceDtos = GetSortByField(request.SortBy, request.IsAscending, serviceDtos);
            
            return ResponseDto.GetSuccess(new
            {
                services = serviceDtos
            }, "Services retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
    
    private static List<ServiceDto> GetSortByField(string sortBy, bool isAscending, List<ServiceDto> serviceDtos)
    {
        return sortBy.ToLower() switch
        {
            "name" => isAscending ? serviceDtos.OrderBy(s => s.Name).ToList() : serviceDtos.OrderByDescending(s => s.Name).ToList(),
            "price" => isAscending ? serviceDtos.OrderBy(s => s.Price).ToList() : serviceDtos.OrderByDescending(s => s.Price).ToList(),
            "opentime" => isAscending ? serviceDtos.OrderBy(s => s.OpenTime).ToList() : serviceDtos.OrderByDescending(s => s.OpenTime).ToList(),
            "closedtime" => isAscending ? serviceDtos.OrderBy(s => s.ClosedTime).ToList() : serviceDtos.OrderByDescending(s => s.ClosedTime).ToList(),
            "status" => isAscending ? serviceDtos.OrderBy(s => s.Status).ToList() : serviceDtos.OrderByDescending(s => s.Status).ToList(),
            _ => isAscending ? serviceDtos.OrderBy(s => s.Price).ToList() : serviceDtos.OrderByDescending(s => s.Price).ToList(),
        };
    }
    
    private static bool IsValidServiceStatus(sbyte status)
    {
        return Enum.IsDefined(typeof(ServiceStatus), status);
    }
}