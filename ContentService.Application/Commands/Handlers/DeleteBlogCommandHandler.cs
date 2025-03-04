using ContentService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteBlogCommandHandler(IBlogRepo blogRepo, ILogger<DeleteBlogCommandHandler> logger) 
    : IRequestHandler<DeleteBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly ILogger<DeleteBlogCommandHandler> _logger = logger;

    public async Task<ResponseDto> Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 DeleteBlogCommand started. BlogId: {BlogId}", request.BlogId);

            var blogIsExisted = await _blogRepo.ExistsAsync(b => b.BlogId == request.BlogId);
            if (!blogIsExisted)
            {
                _logger.LogWarning("❌ Blog not found. BlogId: {BlogId}", request.BlogId);
                return ResponseDto.NotFound("Blog not found");
            }

            var isSucceed = await _blogRepo.ArchiveBlog(request.BlogId);
            if (!isSucceed)
            {
                _logger.LogError("❌ Failed to delete blog. BlogId: {BlogId}", request.BlogId);
                return ResponseDto.InternalError("Failed to delete blog");
            }

            _logger.LogInformation("✅ Blog deleted successfully! BlogId: {BlogId}", request.BlogId);
            return ResponseDto.DeleteSuccess("Blog deleted successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while deleting blog. BlogId: {BlogId}", request.BlogId);
            return ResponseDto.InternalError("Something went wrong while deleting the blog.");
        }
    }
}