using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReplyCommandHandler(IReplyRepo replyRepo, ICommentRepo commentRepo, IUnitOfWork unitOfWork) : IRequestHandler<DeleteReplyCommand, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<ResponseDto> Handle(DeleteReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // fetch comment in db
            var commentIdOfReply = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.ReplyId, r => r.CommentId);
            if (commentIdOfReply <= 0) return ResponseDto.NotFound("Reply not found");

            // delete comment
            var isSucceed = await _replyRepo.DeleteReply(request.ReplyId);

            if (!isSucceed) ResponseDto.InternalError("Failed to delete reply");
            
            // decrease the replies of comment
            await _commentRepo.UpdateFieldsAsync(c => c.CommentId == commentIdOfReply,
                c => c.SetProperty(cc => cc.RepliesCount, cc => cc.RepliesCount - 1));

            await _unitOfWork.CommitTransactionAsync();
            
            return ResponseDto.DeleteSuccess(null, "Reply deleted successfully!");
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            
            return ResponseDto.InternalError(e.Message);
        }
    }
}