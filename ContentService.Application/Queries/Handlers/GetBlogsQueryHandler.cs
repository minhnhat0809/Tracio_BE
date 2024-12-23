using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using LinqKit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogsQueryHandler(IBlogRepo blogRepo) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;

    public async Task<ResponseDto> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var basePredicate = PredicateBuilder.New<Blog>(true);

            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                basePredicate = basePredicate.And(b => b.UserId.Equals(request.UserId));
            }

            if (request.Status.HasValue)
            {
                basePredicate = basePredicate.And(b => b.Status.Equals(request.Status.Value));
            }
            
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                request.SortBy = "CreatedAt";
            }
            
            // count blogs
            var total = await _blogRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(new
            {
                blogs = new List<BlogDtos>(),
                total,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize
            } ,
                "No blogs found");
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Blog>(request.SortBy);

            // fetch blogs
            var blogs = await _blogRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                b => new BlogDtos()
                {
                    BlogId = b.BlogId,
                    UserId = b.UserId,
                    Tittle = b.Tittle,
                    Content = b.Content,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    LikesCount = b.LikesCount,
                    CommentsCount = b.CommentsCount
                },
                request.PageNumber, request.PageSize,
                sortExpression, request.Ascending
            );
            
            return ResponseDto.GetSuccess(new
                {
                    blogs = new List<BlogDtos>(),
                    total,
                    pageNumber = request.PageNumber,
                    pageSize = request.PageSize
                },
                "Blogs retrieved successfully!");
        }
        catch (Exception e)
        {
            return new ResponseDto(null, e.Message, false, StatusCodes.Status500InternalServerError);
        }
    }
}