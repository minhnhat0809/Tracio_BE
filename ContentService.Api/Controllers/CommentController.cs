using ContentService.Application.Commands;
using ContentService.Application.DTOs.CommentDtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromForm] CommentCreateDto commentCreateDto, [FromForm] List<IFormFile> files)
        {
            var result = await _mediator.Send(new CreateCommentCommand(commentCreateDto, files));
            
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
