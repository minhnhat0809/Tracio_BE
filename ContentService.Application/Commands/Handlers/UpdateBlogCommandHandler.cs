using AutoMapper;
using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class UpdateBlogCommandHandler(
    IBlogRepo blogRepo, 
    IModerationService moderationService, 
    IRabbitMqProducer rabbitMqProducer,
    IUserService userService) : IRequestHandler<UpdateBlogCommand, ResponseDto>
{
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IModerationService _moderationService = moderationService;
    
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
    private readonly IUserService _userService = userService;
    
    public async Task<ResponseDto> Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {

            if (!string.IsNullOrWhiteSpace(request.Content) && !string.IsNullOrEmpty(request.Content))
            {
                // moderate content
                var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
                if(!moderationResult.IsSafe) return ResponseDto.BadRequest("Content contains harmful or offensive language.");
                
                // update content
                await _blogRepo.UpdateFieldsAsync(b => b.BlogId == request.BlogId, 
                    b => b.SetProperty(bb => bb.Content, request.Content));
            }

            if (request.PrivacySetting.HasValue)
            {
                // check privacy setting in enum
                if(!IsValidPrivacySetting(request.PrivacySetting.Value)) return ResponseDto.BadRequest("Privacy setting is invalid!");
                
                var privacySetting = await _blogRepo.GetByIdAsync(b => b.BlogId == request.BlogId, b => b.PrivacySetting);
                if (request.PrivacySetting.Value != privacySetting)
                {
                    // get list of followerIds
                    var followerIds = new List<int>();
                    
                    // update privacy setting
                    await _blogRepo.UpdateFieldsAsync(b => b.BlogId == request.BlogId, 
                        b => b.SetProperty(bb => bb.PrivacySetting, request.PrivacySetting.Value));

                    if (request.PrivacySetting == (sbyte)PrivacySetting.FollowerOnly)
                    {
                        // publish an event to add blog in UserFollowerOnlyBlog
                        await _rabbitMqProducer.PublishAsync(new BlogPrivacyUpdateEvent(request.BlogId, followerIds, "add"), "blog_privacy_updated",cancellationToken: cancellationToken);
                    }
                    else
                    {
                        // publish an event to remove blog in UserFollowerOnlyBlog
                        await _rabbitMqProducer.PublishAsync(new BlogPrivacyUpdateEvent(request.BlogId, followerIds, "remove"), "blog_privacy_updated",cancellationToken: cancellationToken);
                    }
                }
            }
            
            return ResponseDto.UpdateSuccess(null, "Blog updated successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
    
    private static bool IsValidPrivacySetting(sbyte privacySetting)
    {
        return Enum.IsDefined(typeof(PrivacySetting), privacySetting);
    }
}