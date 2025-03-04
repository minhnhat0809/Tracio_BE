using AutoMapper;
using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.DTOs.UserDtos.View;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Dtos.Messages;

namespace ContentService.Application.Commands.Handlers;

public class CreateReactionCommandHandler(
    IReactionRepo reactionRepo, 
    IBlogRepo blogRepo,
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo,
    IRabbitMqProducer rabbitMqProducer,
    IUserService userService,
    IMapper mapper,
    IHubContext<ContentHub> hubContext,
    ILogger<CreateReactionCommandHandler> logger,
    ICacheService cacheService) 
    : IRequestHandler<CreateReactionCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IUserService _userService = userService;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IHubContext<ContentHub> _hubContext = hubContext;

    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
    private readonly ILogger<CreateReactionCommandHandler> _logger = logger;
    
    private readonly ICacheService _cacheService = cacheService;
    
    public async Task<ResponseDto> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 CreateReactionCommand started. UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                request.CyclistId, request.EntityId, request.EntityType);
            
            // check userId and get user's name, avatar
            var userDto = await _userService.ValidateUser(request.CyclistId);
            if (userDto.IsUserValid)
                return request.EntityType.ToLower() switch
                {
                    "reply" => await HandleReplyReaction(request, userDto, cancellationToken),
                    "blog" => await HandleBlogReaction(request, userDto, cancellationToken),
                    "comment" => await HandleCommentReaction(request, userDto, cancellationToken),
                    _ => ResponseDto.BadRequest("Invalid entity type. Allowed values: reply, blog, comment.")
                };
            _logger.LogWarning("❌ User validation failed. UserId: {UserId}", request.CyclistId);
            return ResponseDto.NotFound("User does not exist");

        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while creating reaction. UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                request.CyclistId, request.EntityId, request.EntityType);
            return ResponseDto.InternalError("Something went wrong while creating the reaction.");
        }
    }

    private async Task<ResponseDto> HandleReplyReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Processing reply reaction. UserId: {UserId}, ReplyId: {ReplyId}", request.CyclistId, request.EntityId);
        
        // **Cache keys**
        var reactionsCacheKey = $"ContentService_Reactions:Reply{request.EntityId}:*";
        
        var blogAndCommentAndCyclistId = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.EntityId, r => 
            new
            {
                r.CommentId,
                r.CyclistId,
                r.Comment.BlogId
            });
        if (blogAndCommentAndCyclistId == null)
        {
            _logger.LogWarning("❌ Reply not found. ReplyId: {ReplyId}", request.EntityId);
            return ResponseDto.NotFound("Reply not found");
        }

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = userDto.Username,
            CyclistAvatar = userDto.Avatar,
            ReplyId = request.EntityId,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed)
        {
            _logger.LogError("❌ Failed to create reply reaction. ReplyId: {ReplyId}", request.EntityId);
            return ResponseDto.InternalError("Failed to create reaction.");
        }
        
        var reactionDto = _mapper.Map<ReactionDto>(reaction);
        
        // publish new reaction to reply into signalR
        await _hubContext.Clients.Group($"Blog-{blogAndCommentAndCyclistId.BlogId}")
            .SendAsync("ReceiveNewReplyReaction", new
            {
                blogAndCommentAndCyclistId.BlogId,
                blogAndCommentAndCyclistId.CommentId,
                ReplyId = request.EntityId
            }, cancellationToken);

        await _hubContext.Clients.Group($"Comment-{blogAndCommentAndCyclistId.CommentId}")
            .SendAsync("ReceiveNewReplyReaction", new
            {
                blogAndCommentAndCyclistId.CommentId,
                ReplyId = request.EntityId
            }, cancellationToken);
        
        // publish reaction create event
        await _rabbitMqProducer.SendAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "content.reaction.created", cancellationToken);
        
        // publish notification event
        await _rabbitMqProducer.PublishAsync(new NotificationEvent(
            recipientId: blogAndCommentAndCyclistId.CyclistId,
            senderId: request.CyclistId,
            userDto.Username,
            userDto.Avatar,
            $"{userDto.Username} likes your reply",
            request.EntityId,
            "Reply",
            reaction.CreatedAt
        ), cancellationToken: cancellationToken);
        
        // clear cache
        await _cacheService.RemoveByPatternAsync(reactionsCacheKey);

        _logger.LogInformation("✅ Reply reaction created successfully! ReplyId: {ReplyId}, UserId: {UserId}", request.EntityId, request.CyclistId);
        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleBlogReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Processing blog reaction. UserId: {UserId}, BlogId: {BlogId}", request.CyclistId, request.EntityId);
        
        // **Cache keys**
        var reactionsCacheKey = $"ContentService_Reactions:Blog{request.EntityId}:*";
        
        var blogAndCyclist = await _blogRepo.GetByIdAsync(b => b.BlogId == request.EntityId, b => new{b.BlogId, CyclistId = b.BlogId});
        if (blogAndCyclist == null)
        {
            _logger.LogWarning("❌ Blog not found. BlogId: {BlogId}", request.EntityId);
            return ResponseDto.NotFound("Blog not found");
        }

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = userDto.Username,
            CyclistAvatar = userDto.Avatar,
            BlogId = request.EntityId,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed)
        {
            _logger.LogError("❌ Failed to create blog reaction. BlogId: {BlogId}", request.EntityId);
            return ResponseDto.InternalError("Failed to create reaction.");
        }
        
        var reactionDto = _mapper.Map<ReactionDto>(reaction);
        
        // publish new reaction to blog into signalR
        await _hubContext.Clients.Groups($"Blog-{request.EntityId}", "BlogUpdates")
            .SendAsync("ReceiveNewBlogReaction", new
            {
                BlogId = request.EntityId
            }, cancellationToken: cancellationToken);
        
        // publish reaction create event
        await _rabbitMqProducer.SendAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "content.reaction.created", cancellationToken);
        
        // publish notification
        await _rabbitMqProducer.PublishAsync(new NotificationEvent(
            recipientId: blogAndCyclist.CyclistId,
            senderId: request.CyclistId,
            userDto.Username,
            userDto.Avatar,
            $"{userDto.Username} likes your blog",
            request.EntityId,
            "Blog",
            reaction.CreatedAt
        ), cancellationToken: cancellationToken);
        
        // clear cache
        await _cacheService.RemoveByPatternAsync(reactionsCacheKey);
        
        _logger.LogInformation("✅ Blog reaction created successfully! BlogId: {BlogId}, UserId: {UserId}", request.EntityId, request.CyclistId);
        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleCommentReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Processing comment reaction. UserId: {UserId}, CommentId: {CommentId}", request.CyclistId, request.EntityId);
        
        // **Cache keys**
        var reactionsCacheKey = $"ContentService_Reactions:Comment{request.EntityId}:*";

        var blogAndCyclist = await _commentRepo.GetByIdAsync(c => c.CommentId == request.EntityId, c => new { c.BlogId, c.CyclistId });
        if (blogAndCyclist == null)
        {
            _logger.LogWarning("❌ Comment not found. CommentId: {CommentId}", request.EntityId);
            return ResponseDto.NotFound("Comment not found");
        }

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = userDto.Username,
            CyclistAvatar = userDto.Avatar,
            CommentId = request.EntityId,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed)
        {
            _logger.LogError("❌ Failed to create comment reaction. CommentId: {CommentId}", request.EntityId);
            return ResponseDto.InternalError("Failed to create reaction.");
        }
        
        var reactionDto = _mapper.Map<ReactionDto>(reaction);
        
        // publish new reaction to comment into signalR
        await _hubContext.Clients.Group($"Blog-{blogAndCyclist.CyclistId}")
            .SendAsync("ReceiveNewCommentReaction", new
            {
                blogAndCyclist.BlogId,
                CommentId = request.EntityId
            }, cancellationToken: cancellationToken);
        
        await _hubContext.Clients.Group($"Comment-{request.EntityId}")
            .SendAsync("ReceiveNewCommentReaction", new
            {
                CommentId = request.EntityId
            }, cancellationToken: cancellationToken);
        
        // publish reaction create event
        await _rabbitMqProducer.SendAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "content.reaction.created", cancellationToken);
        
        // publish notification
        await _rabbitMqProducer.PublishAsync(new NotificationEvent(
            recipientId: blogAndCyclist.CyclistId,
            senderId: request.CyclistId,
            userDto.Username,
            userDto.Avatar,
            $"{userDto.Username} likes your comment",
            request.EntityId,
            "Comment",
            reaction.CreatedAt
        ), cancellationToken: cancellationToken);
        
        // clear cache
        await _cacheService.RemoveByPatternAsync(reactionsCacheKey);

        _logger.LogInformation("✅ Comment reaction created successfully! CommentId: {CommentId}, UserId: {UserId}", request.EntityId, request.CyclistId);
        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }
}
