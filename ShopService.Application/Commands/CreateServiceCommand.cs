using MediatR;
using Shared.Dtos;
using ShopService.Application.DTOs.ServiceDtos;

namespace ShopService.Application.Commands;

public class CreateServiceCommand(ServiceCreateDto serviceCreateDto): IRequest<ResponseDto>
{
    
}