using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Queries.Handlers;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, ResponseDto>
{
    public Task<ResponseDto> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}