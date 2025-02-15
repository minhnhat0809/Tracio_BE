using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReactionCommandHandler(IReactionRepo reactionRepo) : IRequestHandler<DeleteReactionCommand, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    public async Task<ResponseDto> Handle(DeleteReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch reaction in db
            var replyIsExisted = await _reactionRepo.ExistsAsync(c => c.ReactionId == request.ReactionId);
            if (!replyIsExisted) return ResponseDto.NotFound("Reply not found");

            // delete reaction
            var isSucceed = await _reactionRepo.DeleteAsync(request.ReactionId);
            
            return !isSucceed ? ResponseDto.InternalError("Failed to delete reaction") :
                ResponseDto.DeleteSuccess(null, "Reaction deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}