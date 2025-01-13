using ContentService.Domain.Entities;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateReactionCommandHandler : IRequestHandler<CreateReactionCommand, ResponseDto>
{
    public Task<ResponseDto> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}