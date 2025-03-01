using AutoMapper;
using ContentService.Application.DTOs.NotificationDtos.Message;
using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.DTOs.UserDtos.View;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateReactionCommandHandler(
    IReactionRepo reactionRepo, 
    IBlogRepo blogRepo,
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo,
    IRabbitMqProducer rabbitMqProducer,
    IUserService userService,
    IMapper mapper,
    IHubContext<ContentHub> hubContext) 
    : IRequestHandler<CreateReactionCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IUserService _userService = userService;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IHubContext<ContentHub> _hubContext = hubContext;

    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
    public async Task<ResponseDto> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check userId and get user's name, avatar
            var userDto = await _userService.ValidateUser(request.CyclistId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");
            
            return request.EntityType.ToLower() switch
            {
                "reply" => await HandleReplyReaction(request, userDto, cancellationToken),
                "blog" => await HandleBlogReaction(request, userDto, cancellationToken),
                "comment" => await HandleCommentReaction(request, userDto, cancellationToken),
                _ => ResponseDto.BadRequest("Invalid entity type. Allowed values: reply, blog, comment.")
            };
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<ResponseDto> HandleReplyReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        var blogAndCommentAndCyclistId = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.EntityId, r => 
            new
            {
                r.CommentId,
                r.CyclistId,
                r.Comment.BlogId
            });
        if (blogAndCommentAndCyclistId == null) return ResponseDto.NotFound("Reply not found");

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = userDto.Username,
            CyclistAvatar = userDto.Avatar,
            ReplyId = request.EntityId,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed) return ResponseDto.InternalError("Failed to create reaction.");
        
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
        await _rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "content.created", cancellationToken);
        
        // publish notification event
        await _rabbitMqProducer.PublishAsync(new NotificationEvent(
            recipientId: blogAndCommentAndCyclistId.CyclistId,
            senderId: request.CyclistId,
            userDto.Username,
            userDto.Avatar,
            "",
            request.EntityId,
            "reply",
            reaction.CreatedAt
        ), "content.created", cancellationToken: cancellationToken);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleBlogReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        var blogAndCyclist = await _blogRepo.GetByIdAsync(b => b.BlogId == request.EntityId, b => new{b.BlogId, CyclistId = b.BlogId});
        if (blogAndCyclist  == null) return ResponseDto.NotFound("Blog not found");

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = userDto.Username,
            CyclistAvatar = userDto.Avatar,
            BlogId = request.EntityId,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed) return ResponseDto.InternalError("Failed to create reaction.");
        
        var reactionDto = _mapper.Map<ReactionDto>(reaction);
        
        // publish new reaction to blog into signalR
        await _hubContext.Clients.Groups($"Blog-{request.EntityId}", "BlogUpdates")
            .SendAsync("ReceiveNewBlogReaction", new
            {
                BlogId = request.EntityId
            }, cancellationToken: cancellationToken);
        
        // publish reaction create event
        await _rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "content.created", cancellationToken);
        
        // publish notification
        await _rabbitMqProducer.PublishAsync(new NotificationEvent(
            recipientId: blogAndCyclist.CyclistId,
            senderId: request.CyclistId,
            userDto.Username,
            userDto.Avatar,
            "",
            request.EntityId,
            "blog",
            reaction.CreatedAt
        ), "content.created", cancellationToken: cancellationToken);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleCommentReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        var blogAndCyclist= await _commentRepo.GetByIdAsync(c => c.CommentId == request.EntityId, c => new{c.BlogId, c.CyclistId});
        if (blogAndCyclist == null) return ResponseDto.NotFound("Comment not found");

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = userDto.Username,
            CyclistAvatar = userDto.Avatar,
            CommentId = request.EntityId,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed) return ResponseDto.InternalError("Failed to create reaction.");
        
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
        await _rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "content.created", cancellationToken);
        
        // publish notification
        await _rabbitMqProducer.PublishAsync(new NotificationEvent(
            recipientId: blogAndCyclist.CyclistId,
            senderId: request.CyclistId,
            userDto.Username,
            userDto.Avatar,
            "",
            request.EntityId,
            "comment",
            reaction.CreatedAt
        ), "content.created", cancellationToken: cancellationToken);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }
}
