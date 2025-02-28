using ContentService.Application.Commands;
using ContentService.Application.DTOs.ReplyDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/replies")]
    [ApiController]
    public class ReplyController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReply([FromForm] ReplyCreateDto replyCreateDto, [FromForm] List<IFormFile> files)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);

            var result = await _mediator.Send(new CreateReplyCommand(userBrowsingId, replyCreateDto, files));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{replyId:int}")]
        public async Task<IActionResult> DeleteReply([FromRoute] int replyId)
        {
            var result = await _mediator.Send(new DeleteReplyCommand(replyId));
            
            return StatusCode(result.StatusCode);
        }
    }
}
