using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RouteService.Application.DTOs.RouteMediaFiles;
using RouteService.Application.Interfaces.Services;

namespace RouteService.Api.Controllers
{
    [Route("api/route")]
    [ApiController]
    public class RouteMediaFileController : ControllerBase
    {
        private readonly IRouteMediaFileService _service;
        public RouteMediaFileController(IRouteMediaFileService service)
        {
            _service = service;
        }
        
        /// <summary>
        /// Create Picture In Route | Capture moment to Route while Tracking
        /// </summary>
        [HttpGet("{routeId}/picture")]
        public async Task<IActionResult> GetAllPicturesInRoute(
            [FromRoute] int routeId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            var response = await _service.GetAllRouteMediaFilesAsync(routeId, pageIndex, pageSize, sortBy, sortDesc);
            return StatusCode(response.StatusCode, response);
        }

        
        /// <summary>
        /// Create Picture In Route | Capture moment to Route while Tracking
        /// </summary>
        [HttpGet("{routeId}/picture/{pictureId}")] 
        public async Task<IActionResult> GetPicturesInRoute(
            [FromRoute] int routeId, [FromRoute] int pictureId)
        {
                var response = await _service.GetRouteMediaFileAsync(routeId, pictureId);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// Create Picture In Route | Capture moment to Route while Tracking
        /// </summary>
        [HttpPost("{routeId}/picture")] 
        public async Task<IActionResult> CaptureMomentToRoute(
            [FromRoute] int routeId,
            [FromForm] RouteMediaFileRequestModel requestModel)
        {
            var response = await _service.CreateRouteMediaFileAsync(routeId, requestModel);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// Delete 1 Picture In Route
        /// </summary>
        [HttpDelete("{routeId}/picture/{pictureId}")]  
        public async Task<IActionResult> DeletePictureInRoute(
            [FromRoute] int routeId,
            [FromRoute] int pictureId)
        {
            var response = await _service.DeleteRouteMediaFileAsync(routeId, pictureId);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// Delete All Picture In Route
        /// </summary>
        [HttpDelete("{routeId}/picture")]  
        public async Task<IActionResult> DeleteAllPictureInRoute(
            [FromRoute] int routeId)
        {
            var response = await _service.DeleteAllRouteMediaFileAsync(routeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
