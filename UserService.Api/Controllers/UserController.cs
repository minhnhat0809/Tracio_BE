using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces.Services;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users with pagination, sorting, and filtering
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
            var response = await _userService.GetAllUsersAsync(pageNumber, rowsPerPage, filterField, filterValue, sortField, sortDesc);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int userId)
        {
            var response = await _userService.GetUserByIdAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Get user by property (Email, FirebaseId, or PhoneNumber)
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> GetUserByProperty([FromQuery] string property)
        {
            var response = await _userService.GetUserByPropertyAsync(property);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("{userId:int}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int userId, [FromBody] UpdateUserProfileModel userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _userService.UpdateUserAsync(userId, userModel);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Patch: Update user avatar (partial update)
        /// </summary>
        [HttpPatch("{userId:int}/avatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromRoute] int userId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResponseModel("error", 400, "Invalid file", null));

            var response = await _userService.UpdateUserAvatarAsync(userId, file);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// Delete user account permanently
        /// </summary>
        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int userId)
        {
            var response = await _userService.DeleteAccountAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PATCH: Ban user (Soft delete - sets IsActive to false)
        /// </summary>
        [HttpPatch("{userId:int}/ban")]
        public async Task<IActionResult> BanUser([FromRoute] int userId)
        {
            var response = await _userService.BanUserAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// PATCH: Unban user (Sets IsActive to true)
        /// </summary>
        [HttpPatch("{userId:int}/unban")]
        public async Task<IActionResult> UnBanUser([FromRoute] int userId)
        {
            var response = await _userService.UnBanUserAsync(userId);
            return StatusCode(response.StatusCode, response);
        }

    }
}
