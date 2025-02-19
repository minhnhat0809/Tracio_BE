using AutoMapper;
using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.DTOs.UserDtos.View;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Events;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateReactionCommandHandler(
    IReactionRepo reactionRepo, 
    IBlogRepo blogRepo,
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo,
    IRabbitMqProducer rabbitMqProducer,
    IUserService userService,
    IMapper mapper) 
    : IRequestHandler<CreateReactionCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IUserService _userService = userService;
    
    private readonly IMapper _mapper = mapper;
    
    public async Task<ResponseDto> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check userId and get user's name, avatar
            var userDto = await _userService.ValidateUser(request.CyclistId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");
            
            return request.EntityType.ToLower() switch
            {
                "reply" => await HandleReplyReaction(request, userDto),
                "blog" => await HandleBlogReaction(request, userDto),
                "comment" => await HandleCommentReaction(request, userDto),
                _ => ResponseDto.BadRequest("Invalid entity type. Allowed values: reply, blog, comment.")
            };
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<ResponseDto> HandleReplyReaction(CreateReactionCommand request, UserDto userDto)
    {
        var isReplyExisted = await _replyRepo.ExistsAsync(r => r.ReplyId == request.EntityId);
        if (!isReplyExisted) return ResponseDto.NotFound("Reply not found");

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

        // publish for notification
        //await rabbitMqProducer.PublishAsync(new ReactionToReplyEvent(request.EntityId), "notification_queue");
        
        // publish reaction create event
        await rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "reaction_created");

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleBlogReaction(CreateReactionCommand request, UserDto userDto)
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
        
        // publish for notification
        //await rabbitMqProducer.PublishAsync(new ReactionToBlogEvent(request.EntityId), "notification_queue");
        
        // publish reaction create event
        await rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "reaction_created");

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleCommentReaction(CreateReactionCommand request, UserDto userDto)
    {
        var isCommentExisted= await _commentRepo.ExistsAsync(c => c.CommentId == request.EntityId);
        if (!isCommentExisted) return ResponseDto.NotFound("Comment not found");

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
        
        // publish for notification
        //await rabbitMqProducer.PublishAsync(new ReactionToCommentEvent(request.EntityId), "notification_queue");
        
        // publish reaction create event
        await rabbitMqProducer.PublishAsync(new ReactionCreateEvent(request.EntityId, request.EntityType), "reaction_created");

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }
}
