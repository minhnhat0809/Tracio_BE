using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetCommentsByBlogQueryHandler(
    IBlogRepo blogRepo, 
    ICommentRepo commentRepo, 
    ICacheService cacheService) : IRequestHandler<GetCommentsByBlogQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly ICommentRepo _commentRepo = commentRepo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetCommentsByBlogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber;
            var cacheKeyBlog = $"Blog:{request.BlogId}";
            var cacheKeyComments = $"Comments:Blog{request.BlogId}:Page{pageNumber}:Size{request.PageSize}";

            var blog =
                // **Check if blog details are cached**
                await _cacheService.GetAsync<BlogWithCommentsDto>(cacheKeyBlog);
            if (blog == null)
            {
                // **Fetch blog details from DB**
                blog = await _blogRepo.GetByIdAsync(b => b.BlogId.Equals(request.BlogId), b => new BlogWithCommentsDto
                {
                    BlogId = b.BlogId,
                    CategoryName = b.Category.CategoryName,
                    Content = b.Content,
                    MediaFiles = b.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                    IsReacted = b.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                    IsBookmarked = b.BlogBookmarks.Any(bm => bm.OwnerId == request.UserRequestId),
                    CreatedAt = b.CreatedAt,
                    CommentsCount = b.Comments.Count,
                    LikesCount = b.ReactionsCount,
                    CreatorAvatar = b.CreatorAvatar,
                    CreatorId = b.CreatorId,
                    CreatorName = b.CreatorName
                });

                if (blog == null) return ResponseDto.NotFound($"Blog not found with this id: {request.BlogId}");

                await _cacheService.SetAsync(cacheKeyBlog, blog, TimeSpan.FromMinutes(10)); // Cache for 10 min
            }

            // **Check if paginated comments are cached**
            var cachedComments = await _cacheService.GetAsync<List<CommentDto>>(cacheKeyComments);
            if (cachedComments != null)
            {
                var totalComments = blog.CommentsCount;
                var totalPages1 = (int)Math.Ceiling((double)totalComments / request.PageSize);

                return ResponseDto.GetSuccess(new
                {
                    blog,
                    comments = cachedComments,
                    pageNumber,
                    pageSize = request.PageSize,
                    totalComments,
                    totalPages = totalPages1,
                    hasNextPage = pageNumber < totalPages1,
                    hasPreviousPage = pageNumber > 1
                }, "Comments of the blog retrieved successfully!");
            }

            // **Fetch comments from DB**
            var basePredicate = PredicateBuilder.New<Comment>(true)
                .And(c => c.BlogId.Equals(request.BlogId) && c.IsDeleted != true);

            var total = await _commentRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(new
            {
                comments = new List<CommentDto>(), 
                count = total, 
                pageNumber, 
                pageSize = request.PageSize
            }, "Comments retrieved successfully!");

            var sortExpression = SortHelper.BuildSortExpression<Comment>("CreatedAt");

            var commentsDto = await _commentRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                c => new CommentDto
                {
                    CommentId = c.CommentId,
                    UserId = c.CyclistId,
                    UserName = c.CyclistName,
                    Avatar = c.CyclistAvatar,
                    Content = c.Content,
                    MediaFiles = c.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                    IsReacted = c.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                    CreatedAt = c.CreatedAt,
                    LikesCount = c.LikesCount,
                    RepliesCount = c.RepliesCount
                },
                request.PageNumber, request.PageSize,
                sortExpression, request.IsAscending
            );

            var totalPages2 = (int)Math.Ceiling((double)total / request.PageSize);

            // **Cache the comments for 5 min**
            await _cacheService.SetAsync(cacheKeyComments, commentsDto, TimeSpan.FromMinutes(5));

            return ResponseDto.GetSuccess(new
            {
                blog,
                comments = commentsDto,
                pageNumber,
                pageSize = request.PageSize,
                totalComments = total,
                totalPages = totalPages2,
                hasNextPage = pageNumber < totalPages2,
                hasPreviousPage = pageNumber > 1
            }, "Comments of the blog retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}
