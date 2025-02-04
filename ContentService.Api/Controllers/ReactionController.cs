using ContentService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/reactions")]
    [ApiController]
    public class ReactionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> CreateReaction([FromBody] CreateReactionCommand command)
        {
            var result = await _mediator.Send(command);
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{reactionId:int}")]
        public async Task<IActionResult> DeleteReaction([FromBody] DeleteReactionCommand command)
        {
            var result = await _mediator.Send(command);
            
            return StatusCode(result.StatusCode, result);
        }
    }
}
