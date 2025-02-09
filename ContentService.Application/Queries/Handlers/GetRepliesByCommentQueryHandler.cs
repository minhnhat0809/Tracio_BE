using AutoMapper;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetRepliesByCommentQueryHandler(IMapper mapper, IReplyRepo replyRepo) : IRequestHandler<GetRepliesByCommentQuery, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;

    private readonly IMapper _mapper = mapper;
    
    public Task<ResponseDto> Handle(GetRepliesByCommentQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}