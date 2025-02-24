using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogCategoriesQueryHandler(IBlogCategoryRepo blogCategoryRepo) : IRequestHandler<GetBlogCategoriesQuery, ResponseDto>
{
    private readonly IBlogCategoryRepo _blogCategoryRepo = blogCategoryRepo;
    
    public async Task<ResponseDto> Handle(GetBlogCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryDtos = await _blogCategoryRepo.FindAsync(bc => bc.IsDeleted != true, bc => new BlogCategoryDtos
            {
                CategoryId = bc.CategoryId,
                CategoryName = bc.CategoryName
            });
            
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