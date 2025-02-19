using ContentService.Application.Commands;
using ContentService.Application.DTOs.CommentDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateComment([FromForm] CommentCreateDto commentCreateDto, [FromForm] List<IFormFile> files)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            var result = await _mediator.Send(new CreateCommentCommand(userBrowsingId, commentCreateDto, files));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            var result = await _mediator.Send(new DeleteCommentCommand(commentId));
            
            return StatusCode(result.StatusCode, result);
        }
    }
}
