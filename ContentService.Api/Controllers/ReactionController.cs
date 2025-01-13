using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContentService.Api.Controllers
{
    [Route("api/reactions")]
    [ApiController]
    public class ReactionController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        
        
    }
}
