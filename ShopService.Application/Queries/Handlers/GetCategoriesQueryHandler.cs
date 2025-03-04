using MediatR;
using Shared.Dtos;
using ShopService.Application.DTOs.CategoryDtos.ViewDtos;
using ShopService.Application.Interfaces;

namespace ShopService.Application.Queries.Handlers;

public class GetCategoriesQueryHandler(ICategoryRepo categoryRepo) : IRequestHandler<GetCategoriesQuery, ResponseDto>
{
    private readonly ICategoryRepo _categoryRepo = categoryRepo;
    
    public async Task<ResponseDto> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryDtos = await _categoryRepo.FindAsync(c => c.IsDeleted != true, c =>
                new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName
                });
            
            return ResponseDto.GetSuccess(new
            {
                categories = categoryDtos
            }, "Category retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}