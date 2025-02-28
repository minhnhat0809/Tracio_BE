using AutoMapper;
using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.DTOs.NotificationDtos.Message;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateCommentCommandHandler(
    IMapper mapper, 
    IBlogRepo blogRepo, 
    ICommentRepo commentRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService,
    IRabbitMqProducer rabbitMqProducer,
    IHubContext<ContentHub> hubContext) 
    : IRequestHandler<CreateCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IImageService _imageService = imageService;
    
    private readonly IUserService _userService = userService;
    
    private readonly IModerationService _moderationService = moderationService;
    
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
    private readonly IHubContext<ContentHub> _hubContext = hubContext;
    
    private const string BucketName = "blogtracio";
    
    public async Task<ResponseDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check blog in db
            var blogAndCyclist = await _blogRepo.GetByIdAsync(b => b.BlogId == request.BlogId, b => new {b.BlogId, CyclistId = b.CreatorId});
            if (blogAndCyclist == null) return ResponseDto.NotFound("Blog not found");
            
            // check userId and get user's name
            var userDto = await _userService.ValidateUser(request.CreatorId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");
            
            // moderate content
            /*var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
            if(!moderationResult.IsSafe) return ResponseDto.BadRequest("Content contains harmful or offensive language.");*/
            
            // upload file to aws s3 and get url
            var mediaFileUrl = new List<string>();
            if (request.MediaFiles != null && request.MediaFiles.Count != 0)
            {
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }
            
            var mediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();
            
            var comment = _mapper.Map<Comment>(request);
            
            comment.MediaFiles = mediaFiles;
            comment.CyclistName = userDto.Username;
            comment.CyclistAvatar = userDto.Avatar;
            
            // insert comment into db
            var commentCreateResult = await _commentRepo.CreateAsync(comment);
            
            if(!commentCreateResult) return ResponseDto.InternalError("Failed to create comment");
            
            // publish comment create event
            await _rabbitMqProducer.PublishAsync(new CommentCreateEvent(request.BlogId), "content.created", cancellationToken);
            
            // publish new comment into signalR
            await _hubContext.Clients.Groups("BlogUpdates", $"Blog-{blogAndCyclist.BlogId}")
                .SendAsync("ReceiveNewComment", new
                {
                    request.BlogId
                }, cancellationToken: cancellationToken);
            
            // publish notification
            await _rabbitMqProducer.PublishAsync(new NotificationEvent(
                recipientId: blogAndCyclist.CyclistId,
                senderId: request.CreatorId,
                userDto.Username,
                userDto.Avatar,
                request.Content,
                comment.CommentId,
                "comment",
                comment.CreatedAt
            ), "notification_content", cancellationToken: cancellationToken);

            return ResponseDto.CreateSuccess(null, "Comment created successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}