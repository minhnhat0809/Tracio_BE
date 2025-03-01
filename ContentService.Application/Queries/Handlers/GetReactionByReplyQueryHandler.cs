using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionByReplyQueryHandler(IReplyRepo replyRepo, IReactionRepo  reactionRepo) : IRequestHandler<GetReactionByReplyQuery, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    public async Task<ResponseDto> Handle(GetReactionByReplyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ReplyId <= 0) return ResponseDto.BadRequest("ReplyId is required");
            
            var isReplyExisted = await _replyRepo.ExistsAsync(r => r.ReplyId == request.ReplyId);
            if (!isReplyExisted) return ResponseDto.NotFound("Reply not found");
            
            var reactionDtos = await _reactionRepo.FindAsync(r => r.ReplyId == request.ReplyId, 
                r => new ReactionDto
                {
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    CyclistAvatar = r.CyclistAvatar
                }); 
            
            return ResponseDto.GetSuccess(new
            {
                reactions = reactionDtos
            }, "Reactions retrieved successfully!");
            
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}