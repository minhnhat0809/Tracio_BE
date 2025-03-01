using AutoMapper;
using ContentService.Application.DTOs.NotificationDtos.Message;
using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateReplyCommandHandler(
    IMapper mapper, 
    IReplyRepo replyRepo, 
    ICommentRepo commentRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService,
    IRabbitMqProducer rabbitMqProducer,
    IHubContext<ContentHub> hubContext
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
    
    private const string BucketName = "blogtracio";
    
    public async Task<ResponseDto> Handle(CreateReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check comment in db
            var blogAndCommentAndCyclistId = await _commentRepo.GetByIdAsync(c => c.CommentId == request.CommentId, c => new
            {
                c.CommentId,
                c.CyclistId,
                c.BlogId
            });
            if (blogAndCommentAndCyclistId == null) return ResponseDto.NotFound("Comment not found");
            
            // check userId and get user's name
            var userDto = await _userService.ValidateUser(request.CyclistId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");
            
            // moderate content
            /*var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
            if(!moderationResult.IsSafe) return ResponseDto.BadRequest("Content contains harmful or offensive language.");*/
            
            // upload file to aws s3 and get url
            var mediaFileUrl = new List<string>();
            if (request.MediaFiles != null)
            {
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }
            
            var mediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();
            
            var reply = _mapper.Map<Reply>(request);
            
            reply.MediaFiles = mediaFiles;
            reply.CyclistName = userDto.Username;
            reply.CyclistAvatar = userDto.Avatar;
            
            // insert reply into db
            var replyCreateResult = await _replyRepo.CreateAsync(reply);

            if (!replyCreateResult) return ResponseDto.InternalError("Failed to create reply");
            
            // publish reply create event
            await _rabbitMqProducer.PublishAsync(new ReplyCreateEvent(request.CommentId), "content.created", cancellationToken);
            
            // publish notification event
            await _rabbitMqProducer.PublishAsync(new NotificationEvent(
                blogAndCommentAndCyclistId.CyclistId,
                request.CyclistId,
                userDto.Username,
                userDto.Avatar,
                request.Content,
                reply.ReplyId,
                "reply",
                reply.CreatedAt
                ), "content.created", cancellationToken);
            
            // publish realtime
            await _hubContext.Clients.Group($"Blog-{blogAndCommentAndCyclistId.BlogId}").SendAsync(
                "ReceiveNewReply", new
                {
                    blogAndCommentAndCyclistId.BlogId,
                    blogAndCommentAndCyclistId.CommentId
                }, cancellationToken: cancellationToken);
            
            
            await _hubContext.Clients.Group($"Comment-{blogAndCommentAndCyclistId.CommentId}").SendAsync(
                "ReceiveNewReply", new
                {
                    blogAndCommentAndCyclistId.CommentId
                }, cancellationToken: cancellationToken);
            
            return ResponseDto.CreateSuccess(null, "Reply created successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}