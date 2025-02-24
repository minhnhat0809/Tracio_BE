using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Commands.Handlers;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}