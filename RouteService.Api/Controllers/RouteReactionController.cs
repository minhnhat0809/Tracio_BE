using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RouteService.Application.DTOs.Reacts;
using RouteService.Application.Interfaces.Services;

namespace RouteService.Api.Controllers
{
    
    [Route("api/route")]
    [ApiController]
    public class RouteReactionController : ControllerBase
    {
        
        private readonly IReactService _service;
        
        public RouteReactionController(IReactService service)
        {
            _service = service;
        }
        
        /// <summary>
        /// Get Reactions by Route Id
        /// </summary>
        [HttpGet("{routeId}/react")]
        public async Task<IActionResult> GetAllReactInRoute(
            [FromRoute] int routeId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int rowsPerPage = 10, 
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValue = null,
            [FromQuery] string? sortField = null, 
            [FromQuery] bool sortDesc = false)
        {
            var response = await _service.GetAllRouteReactsAsync( routeId,
                pageNumber, rowsPerPage, filterField, filterValue, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// View Reacts in Review
        /// </summary>
        [HttpGet("comment/{commentId}/react")] 
        public async Task<IActionResult> ViewAllReactInReviewInRoute(
            [FromRoute] int commentId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int rowsPerPage = 10, 
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValue = null,
            [FromQuery] string? sortField = null, 
            [FromQuery] bool sortDesc = false)
        {
            var response = await _service.GetAllCommentReactsAsync(commentId, pageNumber, rowsPerPage, filterField, filterValue, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Create React To Route 
        /// </summary>
        [HttpPost("{routeId}/react")]
        public async Task<IActionResult> CreateReactInRoute(
            [FromRoute] int routeId, [FromForm] int cyclistId)
        {
            var response = await _service.CreateReactRouteAsync(cyclistId, routeId);
            return StatusCode(response.StatusCode, response);
        }
        /// <summary>
        /// Delete React In Route 
        /// </summary>
        [HttpDelete("{routeId}/react/{reactId}")]
        public async Task<IActionResult> DeleteReactInRoute([FromRoute] int routeId, [FromRoute] int reactId)
        {
            var response = await _service.DeleteReactRouteAsync(routeId, reactId);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// Create React To Comment 
        /// </summary>
        [HttpPost("comment/{commentId}/react")]
        public async Task<IActionResult> CreateReactInComment(
            [FromRoute] int commentId, [FromForm] int cyclistId)
        {
            var response = await _service.CreateReactCommentAsync(cyclistId,commentId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete React In Comment 
        /// </summary>
        [HttpDelete("comment/{commentId}/react/{reactId}")]
        public async Task<IActionResult> DeleteReactInComment([FromRoute] int commentId, [FromRoute] int reactId)
        {
            var response = await _service.DeleteReactCommentAsync(commentId, reactId);
            return StatusCode(response.StatusCode, response);
        }

        
       

        

        

       

    }
}
