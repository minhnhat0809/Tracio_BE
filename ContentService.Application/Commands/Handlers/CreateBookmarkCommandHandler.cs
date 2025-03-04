using AutoMapper;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateBookmarkCommandHandler(
    IBlogRepo blogRepo, 
    IBookmarkRepo bookmarkRepo, 
    IMapper mapper, 
    IUserService userService,
    ILogger<CreateBookmarkCommandHandler> logger,
    ICacheService cacheService
) : IRequestHandler<CreateBookmarkCommand, ResponseDto>
{
    private readonly IBookmarkRepo _bookmarkRepo = bookmarkRepo;
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly IMapper _mapper = mapper;
    private readonly IUserService _userService = userService;
    private readonly ILogger<CreateBookmarkCommandHandler> _logger = logger; 
    private readonly ICacheService _cacheService = cacheService;
    public async Task<ResponseDto> Handle(CreateBookmarkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 CreateBookmarkCommand started. UserId: {UserId}, BlogId: {BlogId}", request.OwnerId, request.BlogId);
            
            // **Cache key includes user and pagination**
            var cacheKey = $"ContentService_Bookmarks:{request.OwnerId}:*";

            // ✅ 1. Check if blog exists
            var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId == request.BlogId && b.Status != (sbyte) BlogStatus.Archived);
            if (!isBlogExisted)
            {
                _logger.LogWarning("❌ Blog not found. BlogId: {BlogId}", request.BlogId);
                return ResponseDto.NotFound("Blog not found");
            }

            // ✅ 2. Validate user
            var userDto = await _userService.ValidateUser(request.OwnerId);
            if (!userDto.IsUserValid)
            {
                _logger.LogWarning("❌ User validation failed. UserId: {UserId}", request.OwnerId);
                return ResponseDto.NotFound("User does not exist");
            }

            // ✅ 3. Check if the blog is already bookmarked
            var isBookmarked = await _bookmarkRepo.ExistsAsync(bm => bm.BlogId == request.BlogId && bm.OwnerId == request.OwnerId);
            if (isBookmarked)
            {
                _logger.LogWarning("⚠️ Bookmark already exists. UserId: {UserId}, BlogId: {BlogId}", request.OwnerId, request.BlogId);
                return ResponseDto.BadRequest("Bookmark already exists");
            }

            // ✅ 4. Create bookmark
            var bookMark = _mapper.Map<BlogBookmark>(request);
            var isBookmarkSuccess = await _bookmarkRepo.CreateAsync(bookMark);
            
            if (!isBookmarkSuccess)
            {
                _logger.LogError("❌ Failed to create bookmark. UserId: {UserId}, BlogId: {BlogId}", request.OwnerId, request.BlogId);
                return ResponseDto.InternalError("Bookmark blog creation failed");
            }
            
            // clear cache
            await _cacheService.RemoveByPatternAsync(cacheKey);

            _logger.LogInformation("✅ Blog bookmarked successfully! UserId: {UserId}, BlogId: {BlogId}", request.OwnerId, request.BlogId);
            return ResponseDto.CreateSuccess(null, "Bookmark blog successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception occurred while creating a bookmark. UserId: {UserId}, BlogId: {BlogId}", request.OwnerId, request.BlogId);
            return ResponseDto.InternalError("Something went wrong while bookmarking the blog.");
        }
    }
}
