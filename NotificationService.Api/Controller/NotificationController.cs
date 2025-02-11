using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace NotificationService.Api.Controller
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        
    }
}
