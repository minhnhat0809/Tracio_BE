using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteCommentCommandHandler(
    ICommentRepo commentRepo, 
    IRabbitMqProducer rabbitMqProducer,
    ILogger<DeleteCommentCommandHandler> logger
) : IRequestHandler<DeleteCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly ILogger<DeleteCommentCommandHandler> _logger = logger;

    public async Task<ResponseDto> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 DeleteCommentCommand started. CommentId: {CommentId}", request.CommentId);

            var blogIdOfComment = await _commentRepo.GetByIdAsync(
                c => c.CommentId == request.CommentId, 
                c => c.BlogId
            );

            if (blogIdOfComment <= 0)
            {
                _logger.LogWarning("❌ Comment not found. CommentId: {CommentId}", request.CommentId);
                return ResponseDto.NotFound("Comment not found");
            }

            var isSucceed = await _commentRepo.UpdateFieldsAsync(
                c => c.CommentId == request.CommentId,
                c => c.SetProperty(cc => cc.IsDeleted, true)
            );

            if (!isSucceed)
            {
                _logger.LogError("❌ Failed to delete comment. CommentId: {CommentId}", request.CommentId);
                return ResponseDto.InternalError("Failed to delete comment");
            }

            _logger.LogInformation("✅ Comment deleted successfully! CommentId: {CommentId}", request.CommentId);

            await _rabbitMqProducer.SendAsync(
                new CommentDeleteEvent(blogIdOfComment), 
                "content.comment.deleted", 
                cancellationToken
            );

            return ResponseDto.DeleteSuccess("Comment deleted successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while deleting comment. CommentId: {CommentId}", request.CommentId);
            return ResponseDto.InternalError("Something went wrong while deleting the comment.");
        }
    }
}
