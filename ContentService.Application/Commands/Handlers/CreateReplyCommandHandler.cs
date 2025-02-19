using AutoMapper;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateReplyCommandHandler(
    IMapper mapper, 
    IReplyRepo replyRepo, 
    ICommentRepo commentRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService
    ) : IRequestHandler<CreateReplyCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IImageService _imageService = imageService;
    
    private readonly IUserService _userService = userService;
    
    private readonly IModerationService _moderationService = moderationService;
    
    private const string BucketName = "blogtracio";
    
    public async Task<ResponseDto> Handle(CreateReplyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check comment in db
            var isCommentExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
            if (!isCommentExisted) return ResponseDto.NotFound("Comment not found");
            
            // check userId and get user's name
            var userDto = await _userService.ValidateUser(request.CreatorId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");
            
            // moderate content
            var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
            if(!moderationResult.IsSafe) return ResponseDto.BadRequest("Content contains harmful or offensive language.");
            
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
            
            // insert reply into db
            var replyCreateResult = await _replyRepo.CreateAsync(reply);
            
            // update the replies count in comment
            await _commentRepo.UpdateFieldsAsync(c => c.CommentId == request.CommentId,
                c => c.SetProperty(cc => cc.RepliesCount, cc => cc.RepliesCount +1));
            
            return !replyCreateResult ? ResponseDto.InternalError("Failed to create reply") :
                ResponseDto.CreateSuccess(null, "Reply created successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}