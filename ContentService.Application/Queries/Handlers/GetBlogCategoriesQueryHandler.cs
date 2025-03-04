using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogCategoriesQueryHandler(IBlogCategoryRepo blogCategoryRepo, ICacheService cacheService)
    : IRequestHandler<GetBlogCategoriesQuery, ResponseDto>
{
    private const string CacheKey = "blogCategories"; // Define cache key

    public async Task<ResponseDto> Handle(GetBlogCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check Redis cache first
            var cachedCategories = await cacheService.GetAsync<List<BlogCategoryDtos>>(CacheKey);
            if (cachedCategories != null)
            {
                return ResponseDto.GetSuccess(new
                {
                    categories = cachedCategories
                }, "Categories retrieved successfully from cache!");
            }

            // If not in cache, fetch from database
            var categoryDtos = await blogCategoryRepo.FindAsync(bc => bc.IsDeleted != true, bc => new BlogCategoryDtos
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.CategoryName
            });

            // Store in cache with 24-hour expiration
            await cacheService.SetAsync(CacheKey, categoryDtos, TimeSpan.FromHours(24));

            return ResponseDto.GetSuccess(new
            {
                categories = categoryDtos
            }, "Categories retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}