using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Queries.Handlers;

public class GetRepliesByReviewQueryHandler : IRequestHandler<GetRepliesByReviewQuery, ResponseDto>
{
    public Task<ResponseDto> Handle(GetRepliesByReviewQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}