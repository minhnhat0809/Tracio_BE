using AutoMapper;
using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateBlogCommandHandler(
    IMapper mapper, 
    IBlogRepo blogRepo, 
    ICategoryRepo categoryRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService
     ) : IRequestHandler<CreateBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICategoryRepo _categoryRepo = categoryRepo;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IImageService _imageService = imageService;
    
    private readonly IUserService _userService = userService;
    
    private readonly IModerationService _moderationService = moderationService;
    
    private const string BucketName = "blogtracio";
    
    public async Task<ResponseDto> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check userId and get user's name
            var userDto = await _userService.ValidateUser(request.CreatorId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");

            // check category is existed
            var isCategoryExisted = await _categoryRepo.ExistsAsync(c => c.CategoryId == request.CategoryId && c.IsDeleted != true);
            if (!isCategoryExisted) return ResponseDto.NotFound("Category not found!");
            
            // check privacy setting in enum
            if(!IsValidPrivacySetting(request.PrivacySetting)) return ResponseDto.BadRequest("Privacy setting is invalid!");
            
            // check blog status in enum
            if(!IsValidBlogStatus(request.Status)) return ResponseDto.BadRequest("Status is invalid!");
            
            // moderate content
            var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
            if(!moderationResult.IsSafe) return ResponseDto.BadRequest("Content contains harmful or offensive language.");

            // upload file to aws s3 and get url
            var mediaFileUrl = new List<string>();
            if (request.MediaFiles != null)
            {
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }

            var blog = _mapper.Map<Blog>(request);

            var mediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();
            
            blog.MediaFiles = mediaFiles;
            blog.CreatorName = userDto.Username;
            blog.CreatorAvatar = userDto.Avatar;
            
            // insert into db
            var blogCreateResult = await _blogRepo.CreateAsync(blog);
            
            // map to blogDto
            var blogDto = _mapper.Map<BlogWithCommentsDto>(blog);
            
            return !blogCreateResult ? ResponseDto.InternalError("Failed to create blog!") : 
                ResponseDto.CreateSuccess(blogDto, "Blog created successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private static bool IsValidPrivacySetting(sbyte privacySetting)
    {
        return Enum.IsDefined(typeof(PrivacySetting), privacySetting);
    }
    
    private static bool IsValidBlogStatus(sbyte status)
    {
        return Enum.IsDefined(typeof(BlogStatus), status);
    }

}