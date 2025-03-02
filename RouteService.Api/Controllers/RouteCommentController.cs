using Microsoft.AspNetCore.Mvc;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.RouteComment;
using RouteService.Application.Interfaces.Services;

namespace RouteService.Api.Controllers
{
    [Route("api/")]
    [ApiController]
    public class RouteCommentController : ControllerBase
    {
        private readonly ICommentService _service;
        
        public RouteCommentController(ICommentService service)
        {
            _service = service;
        }
        
        /// <summary>
        /// Get Comments by RouteId | Comment Level 1
        /// </summary>
        [HttpGet("comment/route/{routeId}")]
        public async Task<IActionResult> GetCommentsByRouteId(
            [FromRoute] int routeId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int rowsPerPage = 10, 
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValue = null,
            [FromQuery] string? sortField = null, 
            [FromQuery] bool sortDesc = false)
        {
            var response = await _service.GetCommentsByRouteIdAsync( routeId, 
                pageNumber, rowsPerPage, filterField, filterValue, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }

        
        /// <summary>
        /// Get Replies by RouteId | Comment Level 2
        /// </summary>
        [HttpGet("comment/{commentId}/reply")]
        public async Task<IActionResult> GetRepliesByCommentId( 
            [FromRoute] int commentId,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int rowsPerPage = 10, 
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValue = null,
            [FromQuery] string? sortField = null, 
            [FromQuery] bool sortDesc = false)
        {
            var response = await _service.GetRepliesByCommentIdAsync(commentId,
                pageNumber, rowsPerPage, filterField, filterValue, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }
        
        
        /// <summary>
        /// Create Comment To Route 
        /// </summary>
        [HttpPost("comment/route/{routeId}")]
        public async Task<IActionResult> CreateComment(
            [FromRoute] int routeId,
            [FromBody] CommentCreateRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel(null, "Invalid request data", false, 400));
            }
            
            var response = await _service.CreateCommentAsync(routeId, request);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete Comment In Route 
        /// </summary>
        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> DeleteCommentInRoute(
            [FromRoute] int commentId)
        {
            var response = await _service.DeleteCommentAsync(commentId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update Comment In Route
        /// </summary>
        [HttpPut("comment/{commentId}")] 
        public async Task<IActionResult> UpdateCommentInRoute(
            [FromRoute] int commentId,
            [FromBody] CommentUpdateRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel(null, "Invalid request data", false, 400));
            }
            
            var response = await _service.UpdateCommentAsync(commentId, request);
            return StatusCode(response.StatusCode, response);
        }
        
        // Reply in Comment 
        
        /// <summary>
        /// Create Reply To Comment 
        /// </summary>
        [HttpPost("comment/{commentId}/reply")]
        public async Task<IActionResult> CreateReply(
            [FromRoute] int commentId,
            [FromBody] CommentCreateRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel(null, "Invalid request data", false, 400));
            }
            
            var response = await _service.CreateReplyAsync(commentId,request);
            return StatusCode(response.StatusCode, response);
        }
        
        /// <summary>
        /// Update Reply To Comment 
        /// </summary>
        [HttpPut("comment/{commentId}/reply/{replyId}")]
        public async Task<IActionResult> UpdateReply(
            [FromRoute] int commentId,
            [FromRoute] int replyId,
            [FromBody] CommentUpdateRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel(null, "Invalid request data", false, 400));
            }
    
            var response = await _service.UpdateReplyAsync(commentId, replyId, request);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete Reply In Comment 
        /// </summary>
        [HttpDelete("comment/{commentId}/reply/{replyId}")]
        public async Task<IActionResult> DeleteReply(
            [FromRoute] int commentId,
            [FromRoute] int replyId)
        {
            var response = await _service.DeleteReplyAsync(commentId, replyId);
            return StatusCode(response.StatusCode, response);
        }


    }
}
