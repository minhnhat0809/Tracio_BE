using AutoMapper;
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
        var blogId = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.EntityId, r => r.Comment.BlogId);
        if (blogId <= 0) return ResponseDto.NotFound("Reply not found");

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
        await _hubContext.Clients.Group($"Reply-{request.EntityId}")
            .SendAsync("ReceiveNewReplyReaction", new
            {
                BlogId = blogId,
                ReplyId = request.EntityId,
                request.CyclistId,
                CyclistName = userDto.Username,
                CyclistAvatar = userDto.Avatar
            }, cancellationToken: cancellationToken);
        
        // publish reaction create event
        await rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "reaction_created", cancellationToken);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleBlogReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId == request.EntityId);
        if (!isBlogExisted) return ResponseDto.NotFound("Blog not found");

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
        await _hubContext.Clients.Group($"Blog-{request.EntityId}")
            .SendAsync("ReceiveNewBlogReaction", new
            {
                BlogId = request.EntityId,
                request.CyclistId,
                CyclistName = userDto.Username,
                CyclistAvatar = userDto.Avatar
            }, cancellationToken: cancellationToken);
        
        // publish reaction create event
        await rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "reaction_created", cancellationToken);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleCommentReaction(CreateReactionCommand request, UserDto userDto, CancellationToken cancellationToken)
    {
        var blogId= await _commentRepo.GetByIdAsync(c => c.CommentId == request.EntityId, c => c.BlogId);
        if (blogId <= 0) return ResponseDto.NotFound("Comment not found");

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
        
        // publish new reaction to comment into signalR
        await _hubContext.Clients.Group($"Blog-{request.EntityId}")
            .SendAsync("ReceiveNewCommentReaction", new
            {
                BlogId = blogId,
                CommentId = request.EntityId,
                request.CyclistId,
                CyclistName = userDto.Username,
                CyclistAvatar = userDto.Avatar
            }, cancellationToken: cancellationToken);
        
        // publish reaction create event
        await rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "reaction_created", cancellationToken);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }
}
