using AutoMapper;
using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.DTOs.ReplyDtos.View;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Dtos.Messages;

namespace ContentService.Application.Commands.Handlers;

public class CreateReplyCommandHandler(
    IMapper mapper, 
    IReplyRepo replyRepo, 
    ICommentRepo commentRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService,
    IRabbitMqProducer rabbitMqProducer,
    IHubContext<ContentHub> hubContext,
    ILogger<CreateReplyCommandHandler> logger,
    ICacheService cacheService
) : IRequestHandler<CreateReplyCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    private readonly IMapper _mapper = mapper;
    private readonly IReplyRepo _replyRepo = replyRepo;
    private readonly IImageService _imageService = imageService;
    private readonly IUserService _userService = userService;
    private readonly IModerationService _moderationService = moderationService;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly IHubContext<ContentHub> _hubContext = hubContext;
    private readonly ILogger<CreateReplyCommandHandler> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;
    
    private const string BucketName = "blogtracio";

    public async Task<ResponseDto> Handle(CreateReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 CreateReplyCommand started. UserId: {UserId}, CommentId: {CommentId}", request.CyclistId, request.CommentId);
            
            // Cache key
            var repliesCacheKey = $"ContentService_Replies:Comment{request.CommentId}:*";

            var reReply = (CommentId: 0, CyclistId: 0, BlogId: 0);
            
            (int CommentId, int CyclistId, int BlogId) comment;
            
            if (request.ReReplyId.HasValue)
            {
                var result1 = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.ReReplyId.Value, 
                    r => new {r.CommentId, r.CyclistId, r.Comment.BlogId });
                if(result1 == null) return ResponseDto.NotFound("Reply not found");
                
                reReply = (result1.CommentId, result1.CyclistId, result1.BlogId);
                
                comment = (reReply.CommentId, reReply.CyclistId, reReply.BlogId);
            }
            else
            {
                var result2 = await _commentRepo.GetByIdAsync(c => c.CommentId == request.CommentId, c => new
                {
                    c.CommentId,
                    c.CyclistId,
                    c.BlogId
                });

                if (result2 == null)
                {
                    _logger.LogWarning("❌ Comment not found. CommentId: {CommentId}", request.CommentId);
                    return ResponseDto.NotFound("Comment not found");
                }
                
                comment = (result2.CommentId, result2.CyclistId, result2.BlogId);
            }

            var userDto = await _userService.ValidateUser(request.CyclistId);
            if (!userDto.IsUserValid)
            {
                _logger.LogWarning("❌ User validation failed. UserId: {UserId}", request.CyclistId);
                return ResponseDto.NotFound("User does not exist");
            }

            var mediaFileUrl = new List<string>();
            if (request.MediaFiles != null)
            {
                _logger.LogInformation("📂 Uploading {Count} media files for UserId: {UserId}", request.MediaFiles.Count, request.CyclistId);
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }

            var mediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();

            var reply = _mapper.Map<Reply>(request);
            reply.MediaFiles = mediaFiles;
            reply.CyclistName = userDto.Username;
            reply.CyclistAvatar = userDto.Avatar;

            // insert into db
            var replyCreateResult = await _replyRepo.CreateAsync(reply);
            if (!replyCreateResult)
            {
                _logger.LogError("❌ Failed to create reply. UserId: {UserId}, CommentId: {CommentId}", request.CyclistId, request.CommentId);
                return ResponseDto.InternalError("Failed to create reply");
            }

            _logger.LogInformation("✅ Reply created successfully! ReplyId: {ReplyId}, UserId: {UserId}, CommentId: {CommentId}", 
                reply.ReplyId, request.CyclistId, request.CommentId);
            
            // clear cache
            await _cacheService.RemoveByPatternAsync(repliesCacheKey);

            // count in database
            await _rabbitMqProducer.SendAsync(new ReplyCreateEvent(request.CommentId), "content.reply.created", cancellationToken);

            if (request.ReReplyId.HasValue)
            {
                await _rabbitMqProducer.PublishAsync(new NotificationEvent(
                    recipientId: reReply.CyclistId,
                    senderId: request.CyclistId,
                    userDto.Username,
                    userDto.Avatar,
                    $"{userDto.Username} replies to your reply: {request.Content}",
                    reply.ReplyId,
                    "reply",
                    reply.CreatedAt
                ), cancellationToken);
            }
            else
            {
                await _rabbitMqProducer.PublishAsync(new NotificationEvent(
                    recipientId: comment.CyclistId,
                    senderId: request.CyclistId,
                    userDto.Username,
                    userDto.Avatar,
                    $"{userDto.Username} replies to your comment: {request.Content}",
                    reply.ReplyId,
                    "reply",
                    reply.CreatedAt
                ), cancellationToken);
            }
           

            // real-time count
            await _hubContext.Clients.Group($"Blog-{comment.BlogId}").SendAsync(
                "ReceiveNewReply", new
                {
                    comment.BlogId,
                    comment.CommentId
                }, cancellationToken: cancellationToken);

            await _hubContext.Clients.Group($"Comment-{comment.CommentId}").SendAsync(
                "ReceiveNewReply", new
                {
                    comment.CommentId
                }, cancellationToken: cancellationToken);

            var replyDto = _mapper.Map<ReplyDto>(reply);

            return ResponseDto.CreateSuccess(replyDto, "Reply created successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while creating reply. UserId: {UserId}, CommentId: {CommentId}", request.CyclistId, request.CommentId);
            return ResponseDto.InternalError("Something went wrong while creating the reply.");
        }
    }
}
