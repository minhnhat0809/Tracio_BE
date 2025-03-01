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

public class GetCommentsByBlogQueryHandler(IBlogRepo blogRepo, ICommentRepo commentRepo) : IRequestHandler<GetCommentsByBlogQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    public async Task<ResponseDto> Handle(GetCommentsByBlogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var blog = new BlogWithCommentsDto();
            
            // handle the event when user click notification
            var pageNumber = request.PageNumber;
            if (request.CommentId.HasValue)
            {
                var blogIdAndCommentIndex = await _commentRepo.GetCommentIndex(request.CommentId.Value);

                if (!blogIdAndCommentIndex.Equals((-1, -1)))
                {
                    pageNumber = (int)Math.Ceiling((double) blogIdAndCommentIndex.CommentIndex / request.PageSize);
                    
                    request.BlogId = blogIdAndCommentIndex.BlogId;
                }
                
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
                    LikesCount = b.ReactionsCount!.Value,
                    CreatorAvatar = b.CreatorAvatar,
                    CreatorId = b.CreatorId,
                    CreatorName = b.CreatorName
                });
            }
            else
            {
                if (request.BlogId <= 0) return ResponseDto.BadRequest("Blog Id is required");
            
                // check blog in db
                var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId.Equals(request.BlogId));
                
                if (!isBlogExisted) return ResponseDto.NotFound($"Blog not found with this id : {request.BlogId}");
            }
            
            var basePredicate = PredicateBuilder.New<Comment>(true);
            
            // build filter expression
            basePredicate = basePredicate
                .And(c => c.BlogId.Equals(request.BlogId) && c.IsDeleted != true);
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Comment>("CreatedAt");
            
            // count comments 
            var total = await _commentRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(new
                {
                    comments = new List<CommentDto>(), 
                    count = total, 
                    pageNumber, 
                    pageSize = request.PageSize
                }, 
                "Comments retrieved successfully!");
            
            // fetch comments
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
                    LikesCount = c.LikesCount!.Value,
                    RepliesCount = c.RepliesCount!.Value
                },
                request.PageNumber, request.PageSize,
                sortExpression, request.IsAscending
                );

            var totalPages = (int)Math.Ceiling((double)total / request.PageSize);
            
            return ResponseDto.GetSuccess(new
                {
                    blog, 
                    comments = commentsDto,
                    pageNumber, 
                    pageSize = request.PageSize,
                    totalComments = total, 
                    totalPages,
                    hasNextPage = pageNumber < totalPages,
                    hasPreviousPage = pageNumber > 1
                }, 
                "Comments of the blog retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}