using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReplyCommandHandler(
    IReplyRepo replyRepo, 
    IRabbitMqProducer rabbitMqProducer,
    ILogger<DeleteReplyCommandHandler> logger
) : IRequestHandler<DeleteReplyCommand, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly ILogger<DeleteReplyCommandHandler> _logger = logger;

    public async Task<ResponseDto> Handle(DeleteReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 DeleteReplyCommand started. ReplyId: {ReplyId}", request.ReplyId);

            var commentIdOfReply = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.ReplyId, r => r.CommentId);

            if (commentIdOfReply <= 0)
            {
                _logger.LogWarning("❌ Reply not found. ReplyId: {ReplyId}", request.ReplyId);
                return ResponseDto.NotFound("Reply not found");
            }

            var isSucceed = await _replyRepo.UpdateFieldsAsync(
                r => r.ReplyId == request.ReplyId,
                r => r.SetProperty(rr => rr.IsDeleted, true)
            );

            if (!isSucceed)
            {
                _logger.LogError("❌ Failed to delete reply. ReplyId: {ReplyId}", request.ReplyId);
                return ResponseDto.InternalError("Failed to delete reply");
            }

            _logger.LogInformation("✅ Reply deleted successfully! ReplyId: {ReplyId}", request.ReplyId);

            await _rabbitMqProducer.SendAsync(new ReplyDeleteEvent(commentIdOfReply), "content.reply.deleted", cancellationToken);

            return ResponseDto.DeleteSuccess("Reply deleted successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while deleting reply. ReplyId: {ReplyId}", request.ReplyId);
            return ResponseDto.InternalError("Something went wrong while deleting the reply.");
        }
    }
}
