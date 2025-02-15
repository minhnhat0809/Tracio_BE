using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteCommentCommandHandler(ICommentRepo commentRepo) :  IRequestHandler<DeleteCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    public async Task<ResponseDto> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch comment in db
            var commentIsExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
            if (!commentIsExisted) return ResponseDto.NotFound("Comment not found");

            // delete comment
            var isSucceed = await _commentRepo.DeleteComment(request.CommentId);
            
            return !isSucceed ? ResponseDto.InternalError("Failed to delete comment") :
                    ResponseDto.DeleteSuccess(null, "Comment deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}