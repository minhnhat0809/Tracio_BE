using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Commands;
using NotificationService.Application.Dtos.FcmDtos;

namespace NotificationService.Api.Controller
{
    [Route("api/fcm")]
    [ApiController]
    public class FcmController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFcm([FromBody] FcmCreateDto fcmCreateDto)
        {
            var value = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "custom_id")?.Value;
            if (value == null) return StatusCode(StatusCodes.Status401Unauthorized);
            var userBrowsingId = int.Parse(value);
            
            var result = await _mediator.Send(new CreateFcmCommand(fcmCreateDto, userBrowsingId));
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFcm([FromBody] string deviceId)
        {
            var result = await _mediator.Send(new DeleteFcmCommand(deviceId));
            
            return StatusCode(result.StatusCode, result);
        }
    }
}
