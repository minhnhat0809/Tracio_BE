using MediatR;
using Shared.Dtos;

namespace ShopService.Application.Queries;

public class GetServicesQuery(sbyte? status, int categoryId, int shopId, string sortBy, bool isAscending, int pageSize, int pageNumber) : IRequest<ResponseDto>
{
    public sbyte? Status { get; set; } = status;
    public int CategoryId { get; set; } = categoryId;
    
    public int ShopId { get; set; } = shopId;

    public string SortBy { get; set; } = sortBy;
    
    public bool IsAscending { get; set; } = isAscending;
    
    public int PageSize { get; set; } = pageSize;
    
    public int PageNumber { get; set; } = pageNumber;
}