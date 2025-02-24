using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ShopService.Api.Controllers
{
    [Route("api/services")]
    [ApiController]
    public class ServiceController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        
    }
}
