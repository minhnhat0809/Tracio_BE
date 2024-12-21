using ContentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/blogs")]
    [ApiController]
    public class BlogController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        
        [HttpGet]
        public async Task<IActionResult> GetBlogs(
            [FromQuery] string? userId,
            [FromQuery] sbyte? status,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool ascending = true,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1)
        {
                var query = new GetBlogsQuery
                {
                    UserId = userId,
                    Status = status,
                    SortBy = sortBy,
                    Ascending = ascending,
                    PageSize = pageSize,
                    PageNumber = pageNumber
                };

                var result = await _mediator.Send(query);
                return StatusCode(result.StatusCode, result);
        }
    }
}
