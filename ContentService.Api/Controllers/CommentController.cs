using ContentService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost("{commentId:int}")]
        public async Task<IActionResult> UpdateComment([FromRoute] int commentId, [FromBody] UpdateCommentCommand? command)
        {
            if (command == null) return BadRequest("Request body is empty");
            
            command.CommentId = commentId;
            
            var result = await _mediator.Send(command);
            
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
