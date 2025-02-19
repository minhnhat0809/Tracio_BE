using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteCommentCommandHandler(ICommentRepo commentRepo, IBlogRepo blogRepo) :  IRequestHandler<DeleteCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    public async Task<ResponseDto> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch comment in db
            var blogIdOfComment = await _commentRepo.GetByIdAsync(c => c.CommentId == request.CommentId, c => c.BlogId);
            if (blogIdOfComment <= 0) return ResponseDto.NotFound("Comment not found");

            // delete comment
            var isSucceed = await _commentRepo.DeleteComment(request.CommentId);

            if (!isSucceed) ResponseDto.InternalError("Failed to delete comment");
            
            // decrease comment count
            await _blogRepo.UpdateFieldsAsync(b => b.BlogId == blogIdOfComment,
                b => b.SetProperty(bl => bl.CommentsCount, bl => bl.CommentsCount - 1));
            
            return ResponseDto.DeleteSuccess(null, "Comment deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}