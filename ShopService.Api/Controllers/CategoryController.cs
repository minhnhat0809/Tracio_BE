using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopService.Application.Queries;

namespace ShopService.Api.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _mediator.Send(new GetCategoriesQuery());
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpGet("{categoryId:int}/services")]
        public async Task<IActionResult> GetServices(
            [FromRoute] int categoryId,
            [FromQuery] sbyte? status,
            [FromQuery] int shopId,
            [FromQuery] string sortBy = "Price",
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1
        )
        {
            var result = await _mediator.Send(new GetServicesQuery(status, categoryId, shopId, sortBy, isAscending, pageSize, pageNumber));
            
            return StatusCode(result.StatusCode, result);
        }
    }
}
