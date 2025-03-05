using ContentService.Application.Commands;
using ContentService.Application.DTOs.BlogDtos;
using ContentService.Application.DTOs.BookmarkDtos;
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
        public async Task<IActionResult> GetBlogs(
            [FromQuery] int? userId,
            [FromQuery] int? categoryId,
            [FromQuery] sbyte? blogStatus,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool ascending = true,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var query = new GetBlogsQuery
            (userBrowsingId, userId, categoryId, blogStatus, sortBy, ascending, pageSize, pageNumber);

            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);

        }

        [HttpGet("/categories")]
        public async Task<IActionResult> GetBlogCategories()
        {
            var result = await _mediator.Send(new GetBlogCategoriesQuery());
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("/bookmarks")]
        [Authorize]
        public async Task<IActionResult> GetBookmarks(
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1
            )
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var result = await _mediator.Send(new GetBookmarksQuery(userBrowsingId, pageSize, pageNumber));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{blogId:int}/comments")]
        public async Task<IActionResult> GetCommentsByBlogId(
            [FromRoute] int blogId,
            [FromQuery] int? commentId,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            [FromQuery] bool ascending = false
            )
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var query = new GetCommentsByBlogQuery(userBrowsingId, blogId, commentId, pageNumber, pageSize, ascending);
            
            var result = await _mediator.Send(query);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpGet("{blogId:int}/reactions")]
        public async Task<IActionResult> GetReactionsByBlogId(
            [FromRoute] int blogId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var query = new GetReactionsByBlogQuery(blogId, pageNumber, pageSize);
            
            var result = await _mediator.Send(query);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBlog([FromForm] BlogCreateDto blogCreateDto, [FromForm] List<IFormFile> mediaFiles)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var result = await _mediator.Send(new CreateBlogCommand(userBrowsingId, blogCreateDto, mediaFiles));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut
            ("{blogId:int}")]
        public async Task<IActionResult> UpdateBlog(
            [FromRoute] int blogId,
            [FromBody] BlogUpdateDto blogUpdateDto
        )
        {
            var result = await _mediator.Send(new UpdateBlogCommand(blogId, blogUpdateDto));

            return StatusCode(result.StatusCode, result);
        }
        
        [HttpDelete("{blogId:int}")]
        public async Task<IActionResult> DeleteBlog(
            [FromRoute] int blogId
        )
        {
            var result = await _mediator.Send(new DeleteBlogCommand(blogId));

            return StatusCode(result.StatusCode);
        }

        [HttpPost("bookmarks")]
        [Authorize]
        public async Task<IActionResult> BookmarkBlog([FromBody] BookmarkCreateDto bookmarkCreateDto)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var result = await _mediator.Send(new CreateBookmarkCommand(userBrowsingId, bookmarkCreateDto));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("bookmarks/{blogId:int}")]
        [Authorize]
        public async Task<IActionResult> UnBookmarkBlog([FromRoute] int blogId)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var result = await _mediator.Send(new DeleteBookmarkCommand(blogId, userBrowsingId));
            
            return StatusCode(result.StatusCode, result);
        }
    }
}
