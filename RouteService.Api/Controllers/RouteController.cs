using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using RouteService.Application.DTOs.Routes;

namespace RouteService.Api.Controllers
{
    [Route("api/route")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly Application.Interfaces.Services.IRouteService _routeService;
        
        public RouteController(Application.Interfaces.Services.IRouteService routeService)
        {
            _routeService = routeService;
        }
        /// <summary>
        /// Lấy danh sách tất cả các tuyến đường có phân trang
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var response = await _routeService.GetAllRoutesAsync(pageIndex, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Lấy thông tin tuyến đường theo ID
        /// </summary>
        [HttpGet("{id}/map-4d")]
        public async Task<IActionResult> GetRouteMap4DById(int id)
        {
            var response = await _routeService.GetRouteMap4DByIdAsync(id);
            if (response == null)
                return NotFound(new { message = $"Route with ID {id} not found." });

            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Lấy thông tin tuyến đường theo ID
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetRouteDetailById(int id)
        {
            var response = await _routeService.GetRouteDetailByIdAsync(id);
            if (response == null)
                return NotFound(new { message = $"Route with ID {id} not found." });

            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Tạo mới một tuyến đường
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoute([FromBody] RouteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _routeService.CreateRouteAsync(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("start")] 
        public IActionResult StartRoute([FromQuery] int routeId)
        {
            return Ok("Route started.");
        }

        [HttpPost("record")] 
        public IActionResult RecordRoute([FromBody] object locationData)
        {
            return Ok("Route recording in progress.");
        }

        [HttpPost("update-route")] 
        public IActionResult UpdateRoute([FromBody] object updateData)
        {
            return Ok("Route updated successfully.");
        }

        [HttpPost("capture-moment")] 
        public IActionResult CaptureMoment([FromBody] object momentData)
        {
            return Ok("Moment captured successfully.");
        }

        [HttpPost("finish")] 
        public IActionResult EndRoute([FromQuery] int routeId)
        {
            return Ok("Route ended successfully.");
        }

    }
}
