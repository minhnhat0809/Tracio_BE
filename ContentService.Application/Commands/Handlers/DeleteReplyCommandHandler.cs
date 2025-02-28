using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReplyCommandHandler(IReplyRepo replyRepo, IRabbitMqProducer rabbitMqProducer) : IRequestHandler<DeleteReplyCommand, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
    public async Task<ResponseDto> Handle(DeleteReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch comment in db
            var commentIdOfReply = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.ReplyId, r => r.CommentId);
            if (commentIdOfReply <= 0) return ResponseDto.NotFound("Reply not found");

            // delete reply
            var isSucceed = await _replyRepo.UpdateFieldsAsync(r => r.ReplyId == request.ReplyId,
                r => r.SetProperty(rr => rr.IsDeleted, true));

            if (!isSucceed) ResponseDto.InternalError("Failed to delete reply");
            
            // decrease the replies of comment
            await _rabbitMqProducer.PublishAsync(new ReplyDeleteEvent(commentIdOfReply), "content.deleted", cancellationToken);
            
            return ResponseDto.DeleteSuccess("Reply deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}