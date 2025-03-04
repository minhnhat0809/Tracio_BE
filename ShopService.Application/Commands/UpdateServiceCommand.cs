using MediatR;
using Shared.Dtos;
using ShopService.Application.DTOs.ServiceDtos;

namespace ShopService.Application.Commands;

public class UpdateServiceCommand(int serviceId, ServiceUpdateDto serviceUpdateDto) : IRequest<ResponseDto>
{
    
}