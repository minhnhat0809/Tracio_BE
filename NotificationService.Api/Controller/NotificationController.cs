using MediatR;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Queries;

namespace NotificationService.Api.Controller
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("/user/{userId:int}")]
        public async Task<IActionResult> GetUserNotifications(
            [FromRoute] int userId,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1
            )
        {
            var result = await _mediator.Send(new GetNotificationByUserQuery(userId, pageSize, pageNumber));
            
            return StatusCode(result.StatusCode, result);
        }
    }
}
