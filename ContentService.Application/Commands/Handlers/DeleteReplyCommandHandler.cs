using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReplyCommandHandler(IReplyRepo replyRepo) : IRequestHandler<DeleteReplyCommand, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    public async Task<ResponseDto> Handle(DeleteReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch comment in db
            var replyIsExisted = await _replyRepo.ExistsAsync(c => c.ReplyId == request.ReplyId);
            if (!replyIsExisted) return ResponseDto.NotFound("Reply not found");

            // delete comment
            var isSucceed = await _replyRepo.DeleteReply(request.ReplyId);
            
            return !isSucceed ? ResponseDto.InternalError("Failed to delete reply") :
                ResponseDto.DeleteSuccess(null, "Reply deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}