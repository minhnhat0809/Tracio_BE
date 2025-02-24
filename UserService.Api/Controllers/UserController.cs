using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediatR;
using UserService.Application.Commands;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces.Services;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.Queries;
using System.Security.Claims;

namespace UserService.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all users (Admin only) | maybe there could be a AdminViewModel[full data] and UserViewModel in this get all
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int rowsPerPage = 10, 
            [FromQuery] string? filterField = null,
            [FromQuery] string? filterValue = null,
            [FromQuery] string? sortField = null, 
            [FromQuery] bool sortDesc = false)
        {
            var response = await _mediator.Send(new GetAllUsersQuery()
            {
                PageNumber = pageNumber,
                RowsPerPage = rowsPerPage,
                FilterField = filterField,
                FilterValue = filterValue,
                SortField = sortField,
                SortDesc = sortDesc
            });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get user by ID (User can only access their own profile unless admin)
        /// </summary>
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int userId)
        {
            var currentUserId = int.Parse(User.FindFirstValue("custom_id") ?? "0");
            var isAdmin = User.IsInRole("admin");

            if (!isAdmin && currentUserId != userId)
            {
                return Forbid("You are not authorized to view this profile.");
            }

            var response = await _mediator.Send(new GetUserByIdQuery() { UserId = userId });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update user profile (User can only update their own profile)
        /// </summary>
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int userId, [FromBody] UpdateUserProfileModel userModel)
        {
            var currentUserId = int.Parse(User.FindFirstValue("custom_id") ?? "0");

            if (currentUserId != userId)
            {
                return Forbid("You are not authorized to update this profile.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _mediator.Send(new UpdateUserCommand() { UserId = userId, UserModel = userModel });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update user avatar (User can only update their own avatar)
        /// </summary>
        [HttpPatch("{userId:int}/avatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromRoute] int userId, IFormFile? file)
        {
            var currentUserId = int.Parse(User.FindFirstValue("custom_id") ?? "0");

            if (currentUserId != userId)
            {
                return Forbid("You are not authorized to update this avatar.");
            }

            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel("error", 400, "Invalid file", null));

            var response = await _mediator.Send(new UpdateUserAvatarCommand() { UserId = userId, Avatar = file });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Delete user account permanently (Admin only)
        /// </summary>
        [Authorize(Policy = "RequireAdmin")]
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int userId)
        {
            var response = await _mediator.Send(new DeleteUserCommand { UserId = userId });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Ban user (Admin only)
        /// </summary>
        [Authorize(Policy = "RequireAdmin")]
        [HttpPatch("{userId:int}/ban")]
        public async Task<IActionResult> BanUser([FromRoute] int userId)
        {
            var response = await _mediator.Send(new BanUserCommand { UserId = userId });
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Unban user (Admin only)
        /// </summary>
        [Authorize(Policy = "RequireAdmin")]
        [HttpPatch("{userId:int}/unban")]
        public async Task<IActionResult> UnBanUser([FromRoute] int userId)
        {
            var response = await _mediator.Send(new UnBanUserCommand { UserId = userId });
            return StatusCode(response.StatusCode, response);
        }
    }
}
