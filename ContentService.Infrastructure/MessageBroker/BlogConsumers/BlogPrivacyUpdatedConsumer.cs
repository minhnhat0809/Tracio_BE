using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ContentService.Infrastructure.MessageBroker.BlogConsumers;

public class BlogPrivacyUpdatedConsumer(
    IServiceScopeFactory serviceScopeFactory,
    IUserService userService)
    : IConsumer<BlogPrivacyUpdateEvent>
{
    private readonly IUserService _userService = userService; 

    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    

    public async Task Consume(ConsumeContext<BlogPrivacyUpdateEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var followerOnlyBlogRepo = scope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();

        switch (context.Message.Action.ToLower())
        {
            case "add":
                // Fetch follower IDs from UserService
                var followerIds = await _userService.GetFollowingUserIds(context.Message.UserId);

                await followerOnlyBlogRepo.AddFollowerOnlyBlogsAsync(context.Message.BlogId, followerIds);
                break;

            case "remove":
                await followerOnlyBlogRepo.RemoveFollowerOnlyBlogsAsync(context.Message.BlogId);
                break;
        }
        
    }
}