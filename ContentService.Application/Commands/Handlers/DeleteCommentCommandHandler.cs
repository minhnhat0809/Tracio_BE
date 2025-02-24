using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteCommentCommandHandler(ICommentRepo commentRepo, IBlogRepo blogRepo, IRabbitMqProducer rabbitMqProducer) :  IRequestHandler<DeleteCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
    public async Task<ResponseDto> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch comment in db
            var blogIdOfComment = await _commentRepo.GetByIdAsync(c => c.CommentId == request.CommentId, c => c.BlogId);
            if (blogIdOfComment <= 0) return ResponseDto.NotFound("Comment not found");

            // delete comment
            var isSucceed = await _commentRepo.UpdateFieldsAsync(c => c.CommentId == request.CommentId,
                c => c.SetProperty(cc => cc.IsDeleted, true));

            if (!isSucceed) ResponseDto.InternalError("Failed to delete comment");

            // publish comment delete event
            await _rabbitMqProducer.PublishAsync(new CommentDeleteEvent(blogIdOfComment), "content_deleted", cancellationToken);
            
            return ResponseDto.DeleteSuccess("Comment deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}