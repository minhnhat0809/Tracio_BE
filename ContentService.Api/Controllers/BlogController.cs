using ContentService.Application.Commands;
using ContentService.Application.DTOs.BlogDtos;
using ContentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/blogs")]
    [ApiController]
    public class BlogController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBlogs(
            [FromQuery] int userRequestId,
            [FromQuery] int? userId,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool ascending = true,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1)
        {
                var query = new GetBlogsQuery
                {
                    UserRequestId = userRequestId,
                    UserId = userId,
                    SortBy = sortBy,
                    Ascending = ascending,
                    PageSize = pageSize,
                    PageNumber = pageNumber
                };

                var result = await _mediator.Send(query);
                return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{blogId:int}/comments")]
        public async Task<IActionResult> GetCommentsByBlogId(
            [FromRoute] int blogId,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1,
            [FromQuery] bool ascending = true
            )
        {
            var query = new GetCommentsByBlogQuery
            {
                BlogId = blogId,
                PageSize = pageSize,
                PageNumber = pageNumber,
                IsAscending = ascending
            };
            
            var result = await _mediator.Send(query);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpGet("{blogId:int}/reactions")]
        public async Task<IActionResult> GetReactionsByBlogId(
            [FromRoute] int blogId,
            [FromQuery] sbyte reactionType
        )
        {
            var query = new GetReactionsByBlogQuery()
            {
                BlogId = blogId,
                ReactionType = reactionType
            };
            
            var result = await _mediator.Send(query);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromForm] BlogCreateDto blogCreateDto, [FromForm] List<IFormFile> mediaFiles)
        {
            var result = await _mediator.Send(new CreateBlogCommand(blogCreateDto, mediaFiles));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{blogId:int}")]
        public async Task<IActionResult> UpdateBlog(
            [FromRoute] int blogId,
            [FromBody] UpdateBlogCommand updateCommand
        )
        {
            var result = await _mediator.Send(updateCommand);

        return StatusCode(result.StatusCode, result);
        }
        
        [HttpDelete("{blogId:int}")]
        public async Task<IActionResult> DeleteBlog(
            [FromRoute] int blogId
        )
        {
            var result = await _mediator.Send(new DeleteBlogCommand(blogId));

            return StatusCode(result.StatusCode, result);
        }
    }
}
