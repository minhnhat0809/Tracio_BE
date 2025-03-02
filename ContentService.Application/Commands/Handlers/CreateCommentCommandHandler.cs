using AutoMapper;
using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Dtos.Messages;

namespace ContentService.Application.Commands.Handlers;

public class CreateCommentCommandHandler(
    IMapper mapper, 
    IBlogRepo blogRepo, 
    ICommentRepo commentRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService,
    IRabbitMqProducer rabbitMqProducer,
    IHubContext<ContentHub> hubContext,
    ILogger<CreateCommentCommandHandler> logger
) : IRequestHandler<CreateCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    private readonly IMapper _mapper = mapper;
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly IImageService _imageService = imageService;
    private readonly IUserService _userService = userService;
    private readonly IModerationService _moderationService = moderationService;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly IHubContext<ContentHub> _hubContext = hubContext;
    private readonly ILogger<CreateCommentCommandHandler> _logger = logger;
    
    private const string BucketName = "blogtracio";

    public async Task<ResponseDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 CreateCommentCommand started. UserId: {UserId}, BlogId: {BlogId}", request.CreatorId, request.BlogId);

            var blogAndCyclist = await _blogRepo.GetByIdAsync(b => b.BlogId == request.BlogId, 
                b => new { b.BlogId, CyclistId = b.CreatorId });
            if (blogAndCyclist == null)
            {
                _logger.LogWarning("❌ Blog not found. BlogId: {BlogId}", request.BlogId);
                return ResponseDto.NotFound("Blog not found");
            }

            var userDto = await _userService.ValidateUser(request.CreatorId);
            if (!userDto.IsUserValid)
            {
                _logger.LogWarning("❌ User validation failed. UserId: {UserId}", request.CreatorId);
                return ResponseDto.NotFound("User does not exist");
            }

            var mediaFileUrl = new List<string>();
            if (request.MediaFiles != null && request.MediaFiles.Any())
            {
                _logger.LogInformation("📂 Uploading {Count} media files for UserId: {UserId}", request.MediaFiles.Count, request.CreatorId);
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }

            var mediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();

            var comment = _mapper.Map<Comment>(request);
            comment.MediaFiles = mediaFiles;
            comment.CyclistName = userDto.Username;
            comment.CyclistAvatar = userDto.Avatar;

            var commentCreateResult = await _commentRepo.CreateAsync(comment);
            if (!commentCreateResult)
            {
                _logger.LogError("❌ Failed to create comment. UserId: {UserId}, BlogId: {BlogId}", request.CreatorId, request.BlogId);
                return ResponseDto.InternalError("Failed to create comment");
            }

            _logger.LogInformation("✅ Comment created successfully! CommentId: {CommentId}, UserId: {UserId}, BlogId: {BlogId}", 
                comment.CommentId, request.CreatorId, request.BlogId);

            await _rabbitMqProducer.SendAsync(new CommentCreateEvent(request.BlogId), "content.comment.created", cancellationToken);

            await _hubContext.Clients.Groups("BlogUpdates", $"Blog-{blogAndCyclist.BlogId}")
                .SendAsync("ReceiveNewComment", new { request.BlogId }, cancellationToken: cancellationToken);

            await _rabbitMqProducer.PublishAsync(new NotificationEvent(
                recipientId: blogAndCyclist.CyclistId,
                senderId: request.CreatorId,
                userDto.Username,
                userDto.Avatar,
                $"{userDto.Username} commented on your blog: {request.Content}",
                comment.CommentId,
                "Comment",
                comment.CreatedAt
            ), cancellationToken: cancellationToken);

            var commentDto = _mapper.Map<CommentDto>(comment);
            
            return ResponseDto.CreateSuccess(commentDto, "Comment created successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception occurred while creating a comment. UserId: {UserId}, BlogId: {BlogId}", 
                request.CreatorId, request.BlogId);
            return ResponseDto.InternalError("Something went wrong while creating the comment.");
        }
    }
}
