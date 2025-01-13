using AutoMapper;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetRepliesByCommentIdQueryHandler(IMapper mapper, IReplyRepo replyRepo) : IRequestHandler<GetRepliesByCommentIdQuery, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;

    private readonly IMapper _mapper = mapper;
    
    public Task<ResponseDto> Handle(GetRepliesByCommentIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}