using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Queries.Handlers;

public class GetReviewsByServiceQueryHandler : IRequestHandler<GetReviewsByServiceQuery, ResponseDto>
{
    public Task<ResponseDto> Handle(GetReviewsByServiceQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}