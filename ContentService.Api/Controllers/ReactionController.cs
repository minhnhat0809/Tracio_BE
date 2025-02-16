using ContentService.Application.Commands;
using ContentService.Application.DTOs.ReactionDtos;
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
        public async Task<IActionResult> CreateReaction( [FromBody] ReactionCreateDto reactionCreateDto)
        {
            var result = await _mediator.Send(new CreateReactionCommand(reactionCreateDto));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{reactionId:int}")]
        public async Task<IActionResult> DeleteReaction(int reactionId)
        {
            var result = await _mediator.Send(new DeleteReactionCommand(reactionId));
            
            return StatusCode(result.StatusCode);
        }
    }
}
