using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enum;
using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetCommentsByBlogIdQueryHandler(IBlogRepo blogRepo, ICommentRepo commentRepo) : IRequestHandler<GetCommentsByBlogIdQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    public async Task<ResponseDto> Handle(GetCommentsByBlogIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            /*if (string.IsNullOrWhiteSpace(request.BlogId)) return ResponseDto.BadRequest("Blog Id is required");
            
            // check blog in db
            var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId.Equals(request.BlogId));

            if (!isBlogExisted) return ResponseDto.NotFound($"Blog not found with this id : {request.BlogId}");

            var basePredicate = PredicateBuilder.New<Comment>(true);
            
            // build filter expression
            basePredicate = basePredicate
                .And(c => c.EntityType == EntityType.Blog && c.EntityId.Equals(request.BlogId));
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Comment>("CreatedAt");
            
            // count comments 
            var total = await _commentRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(new
                {
                    comments = new List<CommentDto>(), 
                    count = total, 
                    pageNumber = request.PageNumber, 
                    pageSize = request.PageSize
                }, 
                "Comments retrieved successfully!");
            
            // fetch comments
            var commentsDto = await _commentRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                c => new CommentDto()
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsEdited = c.IsEdited,
                    LikesCount = c.LikesCount
                },
                request.PageNumber, request.PageSize,
                sortExpression, request.IsAscending
                );*/
            
            return ResponseDto.GetSuccess(new
                {
                    comments = new List<CommentDto>(), 
                    total = 0, 
                    pageNumber = request.PageNumber, 
                    pageSize = request.PageSize
                }, 
                "Comments retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}