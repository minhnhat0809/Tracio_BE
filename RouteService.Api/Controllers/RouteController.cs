using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using RouteService.Application.DTOs.Routes;
using RouteService.Application.Interfaces.Services;

namespace RouteService.Api.Controllers
{
    namespace RouteService.Api.Controllers
{
    [Route("api/route")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routeService;

        public RouteController(IRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int rowsPerPage = 10,
            [FromQuery] Dictionary<string, string>? filters = null,
            [FromQuery] string? sortField = null,
            [FromQuery] bool sortDesc = false)
        {
            var response = await _routeService.GetAllRoutesAsync(pageNumber, rowsPerPage, filters, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{routeId}")]
        public async Task<IActionResult> GetRouteById([FromRoute] int routeId)
        {
            var response = await _routeService.GetRouteByIdAsync(routeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] RouteCreateRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _routeService.CreateRouteAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{routeId}")]
        public async Task<IActionResult> UpdateRoute([FromRoute] int routeId, [FromBody] RouteUpdateRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _routeService.UpdateRouteAsync(routeId, request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{routeId}")]
        public async Task<IActionResult> SoftDeleteRoute([FromRoute] int routeId)
        {
            var response = await _routeService.SoftDeleteRouteAsync(routeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{routeId}/start")]
        public async Task<IActionResult> StartTrackingRoute([FromRoute] int routeId)
        {
            var response = await _routeService.StartTrackingAsync(routeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{routeId}/tracking")]
        public async Task<IActionResult> TrackingInRoute([FromRoute] int routeId, [FromBody] TrackingRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _routeService.TrackingInRouteAsync(routeId, request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{routeId}/finish")]
        public async Task<IActionResult> FinishTrackingRoute([FromRoute] int routeId)
        {
            var response = await _routeService.FinishTrackingAsync(routeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}

}
