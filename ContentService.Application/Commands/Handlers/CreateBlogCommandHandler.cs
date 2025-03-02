using AutoMapper;
using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateBlogCommandHandler(
    IMapper mapper, 
    IBlogRepo blogRepo, 
    ICategoryRepo categoryRepo, 
    IImageService imageService,
    IUserService userService,
    IModerationService moderationService,
    ILogger<CreateBlogCommandHandler> logger
) : IRequestHandler<CreateBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly ICategoryRepo _categoryRepo = categoryRepo;
    private readonly IMapper _mapper = mapper;
    private readonly IImageService _imageService = imageService;
    private readonly IUserService _userService = userService;
    private readonly IModerationService _moderationService = moderationService;
    private readonly ILogger<CreateBlogCommandHandler> _logger = logger; 

    private const string BucketName = "blogtracio";

    public async Task<ResponseDto> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 CreateBlogCommand started for UserId: {UserId}", request.CreatorId);

            // ✅ 1. Validate user
            var userDto = await _userService.ValidateUser(request.CreatorId);
            if (!userDto.IsUserValid)
            {
                _logger.LogWarning("❌ User validation failed. UserId: {UserId}", request.CreatorId);
                return ResponseDto.NotFound("User does not exist");
            }

            // ✅ 2. Validate category
            var isCategoryExisted = await _categoryRepo.ExistsAsync(c => c.CategoryId == request.CategoryId && c.IsDeleted != true);
            if (!isCategoryExisted)
            {
                _logger.LogWarning("❌ Category not found. CategoryId: {CategoryId}", request.CategoryId);
                return ResponseDto.NotFound("Category not found!");
            }

            // ✅ 3. Validate Privacy Setting & Status
            if (!IsValidPrivacySetting(request.PrivacySetting))
            {
                _logger.LogWarning("⚠️ Invalid privacy setting: {PrivacySetting}", request.PrivacySetting);
                return ResponseDto.BadRequest("Privacy setting is invalid!");
            }

            if (!IsValidBlogStatus(request.Status))
            {
                _logger.LogWarning("⚠️ Invalid blog status: {Status}", request.Status);
                return ResponseDto.BadRequest("Status is invalid!");
            }

            // ✅ 4. Moderate content
            var moderationResult = await _moderationService.ProcessModerationResult(request.Content);
            if (!moderationResult.IsSafe)
            {
                _logger.LogWarning("⚠️ Content moderation failed for UserId: {UserId}", request.CreatorId);
                return ResponseDto.BadRequest("Content contains harmful or offensive language.");
            }

            // ✅ 5. Upload media files (Only log if there are media files)
            var mediaFileUrl = new List<string>();
            if (request.MediaFiles != null && request.MediaFiles.Any())
            {
                _logger.LogInformation("📂 Uploading {Count} media files for UserId: {UserId}", request.MediaFiles.Count, request.CreatorId);
                mediaFileUrl = await _imageService.UploadFiles(request.MediaFiles, BucketName, null);
            }

            // ✅ 6. Map & create blog
            var blog = _mapper.Map<Blog>(request);
            blog.MediaFiles = mediaFileUrl.Select(file => new MediaFile { MediaUrl = file, UploadedAt = DateTime.Now }).ToList();
            blog.CreatorName = userDto.Username;
            blog.CreatorAvatar = userDto.Avatar;

            var blogCreateResult = await _blogRepo.CreateAsync(blog);
            if (!blogCreateResult)
            {
                _logger.LogError("❌ Failed to create blog. UserId: {UserId}", request.CreatorId);
                return ResponseDto.InternalError("Failed to create blog!");
            }

            _logger.LogInformation("✅ Blog created successfully! BlogId: {BlogId}, UserId: {UserId}", blog.BlogId, request.CreatorId);

            var blogDto = _mapper.Map<BlogWithCommentsDto>(blog);
            return ResponseDto.CreateSuccess(blogDto, "Blog created successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception occurred while creating a blog for UserId: {UserId}", request.CreatorId);
            return ResponseDto.InternalError("Something went wrong while creating the blog.");
        }
    }

    private static bool IsValidPrivacySetting(sbyte privacySetting) => Enum.IsDefined(typeof(PrivacySetting), privacySetting);
    private static bool IsValidBlogStatus(sbyte status) => Enum.IsDefined(typeof(BlogStatus), status);
}
