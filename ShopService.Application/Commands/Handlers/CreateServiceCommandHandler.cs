using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Commands.Handlers;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}