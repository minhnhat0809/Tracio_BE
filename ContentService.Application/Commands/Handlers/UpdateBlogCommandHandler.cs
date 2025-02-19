using AutoMapper;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class UpdateBlogCommandHandler(IBlogRepo blogRepo, IModerationService moderationService, IRabbitMqProducer rabbitMqProducer) : IRequestHandler<UpdateBlogCommand, ResponseDto>
{
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IModerationService _moderationService = moderationService;
    
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
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
                    // update privacy setting
                    await _blogRepo.UpdateFieldsAsync(b => b.BlogId == request.BlogId, 
                        b => b.SetProperty(bb => bb.PrivacySetting, request.PrivacySetting.Value));
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