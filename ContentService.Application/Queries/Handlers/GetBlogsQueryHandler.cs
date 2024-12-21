using System.Linq.Expressions;
using ContentService.Application.DTOs;
using ContentService.Application.DTOs.BlogDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using LinqKit;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogsQueryHandler(IBlogRepo blogRepo) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;

    public async Task<ResponseDto> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var basePredicate = PredicateBuilder.New<Blog>(true);

            if (string.IsNullOrWhiteSpace(request.UserId))
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
            
            var sortExpression = GetSortExpression(request.SortBy);

            var blogDtos = await _blogRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                b => new BlogViewDto()
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
            
            return new ResponseDto(blogDtos, "Blogs retrieved successfully.", true, StatusCodes.Status200OK);
        }
        catch (Exception e)
        {
            return new ResponseDto(null, e.Message, false, StatusCodes.Status500InternalServerError);
        }
    }
    
    private static Expression<Func<Blog, object>> GetSortExpression(string sortField)
    {
        return sortField.ToLower() switch
        {
            "CreatedAt" => b => b.CreatedAt,
            "UpdatedAt" => b => b.UpdatedAt,
            "LikesCount" => b => b.LikesCount,
            "CommentsCount" => b => b.CommentsCount,
            _ => throw new ArgumentException($"Invalid sort field: {sortField}")
        };
    }
}