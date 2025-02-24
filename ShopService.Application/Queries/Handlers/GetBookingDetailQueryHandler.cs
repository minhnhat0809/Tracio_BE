using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Queries.Handlers;

public class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, ResponseDto>
{
    public Task<ResponseDto> Handle(GetBookingDetailQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}