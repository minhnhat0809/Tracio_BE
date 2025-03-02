using ContentService.Application.Commands;
using ContentService.Application.DTOs.ReactionDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/reactions")]
    [ApiController]
    public class ReactionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> CreateReaction( [FromBody] ReactionCreateDto reactionCreateDto)
        {
            /*var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);*/
            
            var result = await _mediator.Send(new CreateReactionCommand(4, reactionCreateDto));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{entityId:int}")]
        public async Task<IActionResult> DeleteReaction([FromRoute] int entityId, [FromQuery] string entityType)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var result = await _mediator.Send(new DeleteReactionCommand(userBrowsingId, entityId, entityType));
            
            return StatusCode(result.StatusCode);
        }
    }
}
