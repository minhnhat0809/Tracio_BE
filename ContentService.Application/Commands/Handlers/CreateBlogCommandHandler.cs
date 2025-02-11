using AutoMapper;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateBlogCommandHandler(IMapper mapper, IBlogRepo blogRepo, ICategoryRepo categoryRepo, IImageService imageService) : IRequestHandler<CreateBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICategoryRepo _categoryRepo = categoryRepo;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IImageService _imageService = imageService;
    
    private const string BucketName = "blogtracio";
    
    public async Task<ResponseDto> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mediaFileUrl = new List<string>();

            // check category is existed
            var isCategoryExisted = await _categoryRepo.ExistsAsync(c => c.CategoryId == request.CategoryId && c.IsDeleted != true);
            if (!isCategoryExisted) return ResponseDto.NotFound("Category not found!");
            
            // check privacy setting in enum
            if(!IsValidPrivacySetting(request.PrivacySetting)) return ResponseDto.BadRequest("Privacy setting is invalid!");
            
            // check blog status in enum
            if(!IsValidBlogStatus(request.Status)) return ResponseDto.BadRequest("Status is invalid!");

            // upload file to aws s3 and get url
            if (request.MediaFiles != null)
            {
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }

            var blog = _mapper.Map<Blog>(request);

            var mediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();
            
            blog.MediaFiles = mediaFiles;
            
            // insert to db
            var blogCreateResult = await _blogRepo.CreateAsync(blog);
            
            return !blogCreateResult ? ResponseDto.InternalError("Failed to create blog!") : 
                ResponseDto.CreateSuccess(null, "Blog created successfully!");
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