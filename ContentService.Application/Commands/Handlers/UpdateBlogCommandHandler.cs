using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using ContentService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class UpdateBlogCommandHandler(
    IBlogRepo blogRepo, 
    IModerationService moderationService, 
    IRabbitMqProducer rabbitMqProducer,
    ILogger<UpdateBlogCommandHandler> logger
) : IRequestHandler<UpdateBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly IModerationService _moderationService = moderationService;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly ILogger<UpdateBlogCommandHandler> _logger = logger;

    public async Task<ResponseDto> Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 UpdateBlogCommand started. BlogId: {BlogId}", request.BlogId);

            if (!string.IsNullOrWhiteSpace(request.Content) && !string.IsNullOrEmpty(request.Content))
            {
                _logger.LogInformation("🔍 Moderating content for BlogId: {BlogId}", request.BlogId);

                var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
                if (!moderationResult.IsSafe)
                {
                    _logger.LogWarning("❌ Content moderation failed for BlogId: {BlogId}. Content contains harmful language.", request.BlogId);
                    return ResponseDto.BadRequest("Content contains harmful or offensive language.");
                }

                _logger.LogInformation("✅ Content moderation passed for BlogId: {BlogId}. Updating content.", request.BlogId);

                await _blogRepo.UpdateFieldsAsync(
                    b => b.BlogId == request.BlogId, 
                    b => b.SetProperty(bb => bb.Content, request.Content)
                );
            }

            if (!request.PrivacySetting.HasValue) 
            {
                _logger.LogInformation("✅ Blog updated successfully! BlogId: {BlogId}, No privacy setting changed.", request.BlogId);
                return ResponseDto.UpdateSuccess(null, "Blog updated successfully!");
            }

            if (!IsValidPrivacySetting(request.PrivacySetting.Value))
            {
                _logger.LogWarning("❌ Invalid privacy setting for BlogId: {BlogId}. Value: {PrivacySetting}", request.BlogId, request.PrivacySetting);
                return ResponseDto.BadRequest("Privacy setting is invalid!");
            }

            var privacySetting = await _blogRepo.GetByIdAsync(b => b.BlogId == request.BlogId, b => b.PrivacySetting);
            if (request.PrivacySetting.Value == privacySetting)
            {
                _logger.LogInformation("✅ Blog privacy setting is unchanged. BlogId: {BlogId}", request.BlogId);
                return ResponseDto.UpdateSuccess(null, "Blog updated successfully!");
            }

            var userId = await _blogRepo.GetByIdAsync(b => b.BlogId == request.BlogId, b => b.CreatorId);
            
            _logger.LogInformation("🔄 Updating privacy setting for BlogId: {BlogId}. New Privacy: {PrivacySetting}", request.BlogId, request.PrivacySetting);

            await _blogRepo.UpdateFieldsAsync(
                b => b.BlogId == request.BlogId, 
                b => b.SetProperty(bb => bb.PrivacySetting, request.PrivacySetting.Value)
            );

            if (request.PrivacySetting == (sbyte)PrivacySetting.FollowerOnly)
            {
                _logger.LogInformation("📢 Publishing BlogPrivacyUpdateEvent to ADD blog in UserFollowerOnlyBlog. BlogId: {BlogId}", request.BlogId);
                await _rabbitMqProducer.PublishAsync(new BlogPrivacyUpdateEvent(userId, request.BlogId, "add"), cancellationToken: cancellationToken);
            }
            else
            {
                _logger.LogInformation("📢 Publishing BlogPrivacyUpdateEvent to REMOVE blog from UserFollowerOnlyBlog. BlogId: {BlogId}", request.BlogId);
                await _rabbitMqProducer.PublishAsync(new BlogPrivacyUpdateEvent(userId, request.BlogId, "remove"), cancellationToken: cancellationToken);
            }

            _logger.LogInformation("✅ Blog updated successfully! BlogId: {BlogId}", request.BlogId);
            return ResponseDto.UpdateSuccess(null, "Blog updated successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while updating blog. BlogId: {BlogId}", request.BlogId);
            return ResponseDto.InternalError("Something went wrong while updating the blog.");
        }
    }
    
    private static bool IsValidPrivacySetting(sbyte privacySetting)
    {
        return Enum.IsDefined(typeof(PrivacySetting), privacySetting);
    }
}
