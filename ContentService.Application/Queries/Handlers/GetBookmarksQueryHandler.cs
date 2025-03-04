using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetBookmarksQueryHandler(IBookmarkRepo bookmarkRepo, ICacheService cacheService) 
    : IRequestHandler<GetBookmarksQuery, ResponseDto>
{
    private readonly IBookmarkRepo _bookmarkRepo = bookmarkRepo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetBookmarksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // **Cache key includes user and pagination**
            var cacheKey = $"Bookmarks:{request.OwnerId}:Page{request.PageNumber}:Size{request.PageSize}";

            // **Check cache first**
            var cachedBookmarks = await _cacheService.GetAsync<List<BlogDtos>>(cacheKey);
            if (cachedBookmarks != null)
            {
                return ResponseDto.GetSuccess(new
                {
                    blogs = cachedBookmarks,
                    pageSize = request.PageSize,
                    pageNumber = request.PageNumber
                }, "Bookmarks retrieved successfully from cache!");
            }

            // **Fetch from DB if not in cache**
            var blogDtos = await _bookmarkRepo.FindAsyncWithPagingAndSorting(
                bm => bm.OwnerId == request.OwnerId, 
                bm => new BlogDtos
                {
                    BlogId = bm.Blog.BlogId,
                    UserId = bm.Blog.CreatorId,
                    UserName = bm.Blog.CreatorName,
                    Avatar = bm.Blog.CreatorAvatar,
                    PrivacySetting = bm.Blog.PrivacySetting,
                    Content = bm.Blog.Content,
                    MediaFiles = bm.Blog.MediaFiles.Select(mf => new MediaFileDto 
                    { 
                        MediaId = mf.MediaId, 
                        MediaUrl = mf.MediaUrl 
                    }).ToList(),
                    CreatedAt = bm.Blog.CreatedAt,
                    UpdatedAt = bm.Blog.UpdatedAt,
                    LikesCount = bm.Blog.ReactionsCount,
                    CommentsCount = bm.Blog.CommentsCount,
                    IsReacted = bm.Blog.Reactions.Any(r => r.CyclistId == request.OwnerId)
                },
                request.PageNumber, 
                request.PageSize,
                includes: [
                    bm => bm.Blog.MediaFiles, 
                    bm => bm.Blog.Reactions
                ]
            );

            // **Cache results for 10 minutes**
            await _cacheService.SetAsync(cacheKey, blogDtos, TimeSpan.FromMinutes(10));

            return ResponseDto.GetSuccess(new
            {
                blogs = blogDtos,
                pageSize = request.PageSize,
                pageNumber = request.PageNumber
            }, "Bookmarks retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}
