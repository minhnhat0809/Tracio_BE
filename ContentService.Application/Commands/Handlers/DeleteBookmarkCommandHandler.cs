using ContentService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteBookmarkCommandHandler(IBookmarkRepo bookmarkRepo, ILogger<DeleteBookmarkCommandHandler> logger) 
    : IRequestHandler<DeleteBookmarkCommand, ResponseDto>
{
    private readonly IBookmarkRepo _bookmarkRepo = bookmarkRepo;
    private readonly ILogger<DeleteBookmarkCommandHandler> _logger = logger;

    public async Task<ResponseDto> Handle(DeleteBookmarkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 DeleteBookmarkCommand started. UserId: {UserId}, BlogId: {BlogId}", 
                request.UserRequestId, request.BlogId);

            var bookmarkId = await _bookmarkRepo.GetByIdAsync(
                bm => bm.BlogId == request.BlogId && bm.OwnerId == request.UserRequestId, 
                bm => bm.BookmarkId
            );

            if (bookmarkId <= 0)
            {
                _logger.LogWarning("❌ Bookmark not found. UserId: {UserId}, BlogId: {BlogId}", 
                    request.UserRequestId, request.BlogId);
                return ResponseDto.NotFound("Bookmark not found");
            }

            var result = await _bookmarkRepo.DeleteAsync(bookmarkId);
            if (!result)
            {
                _logger.LogError("❌ Failed to delete bookmark. BookmarkId: {BookmarkId}, UserId: {UserId}, BlogId: {BlogId}", 
                    bookmarkId, request.UserRequestId, request.BlogId);
                return ResponseDto.InternalError("Failed to delete bookmark");
            }

            _logger.LogInformation("✅ Bookmark deleted successfully! BookmarkId: {BookmarkId}, UserId: {UserId}, BlogId: {BlogId}", 
                bookmarkId, request.UserRequestId, request.BlogId);
            return ResponseDto.DeleteSuccess("Bookmark deleted successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while deleting bookmark. UserId: {UserId}, BlogId: {BlogId}", 
                request.UserRequestId, request.BlogId);
            return ResponseDto.InternalError("Something went wrong while deleting the bookmark.");
        }
    }
}
