using Microsoft.AspNetCore.Mvc;
using RouteService.Application.Interfaces.Services;

namespace RouteService.Api.Controllers
{
    
    [Route("api/route")]
    [ApiController]
    public class RouteBookMarkController : ControllerBase
    {
        private readonly IRouteBookmarkService _service;
        public RouteBookMarkController(IRouteBookmarkService service)
        {
            _service = service;
        }
    
        /// <summary>
        /// Get All Routes In Bookmark
        /// </summary>
        [HttpGet("user/{userId}/bookmark/route")]
        public async Task<IActionResult> GetAllRoutesInBookmark(
            [FromRoute] int userId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int rowsPerPage = 10, 
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValue = null,
            [FromQuery] string? sortField = null, 
            [FromQuery] bool sortDesc = false)
        {
            var response = await _service.GetAllRouteBookmarksAsync(
                userId, pageNumber, rowsPerPage, 
                filterField, filterValue, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Add Route To Bookmark
        /// </summary>
        [HttpPost("user/{userId}/bookmark/route/{routeId}")]
        public async Task<IActionResult> AddRouteToBookMark(
            [FromRoute] int userId,
            [FromRoute] int routeId)
        {
            var response = await _service.CreateRouteBookmarkAsync(userId, routeId);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// UnBookmark Route
        /// </summary>
        [HttpDelete("user/{userId}/bookmark/route/{routeId}")]
        public async Task<IActionResult> UnBookMarkRoute(
            [FromRoute] int userId,
            [FromRoute] int routeId)
        {
            var response = await _service.DeleteRouteBookmarkAsync(userId, routeId);
            return StatusCode(response.StatusCode, response);
        }


    }
}
