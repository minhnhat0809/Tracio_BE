using Firebase.Auth.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces.Services;

namespace UserService.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestModel loginRequest) // ✅ Expect JSON request body
        {
            if (loginRequest == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _authService.Login(loginRequest);
            return Ok(response);
        }

        // POST: api/auth/register-user (Register a new user)
        [HttpPost("register-user")]
        public async Task<ActionResult> RegisterUser([FromForm] UserRegisterModel request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _authService.Register(request);

            return Ok(response);
        }
        
        // POST: api/auth/register-shop (Register a new user)
        [HttpPost("register-shop")]
        public async Task<ActionResult> RegisterShop([FromForm] UserRegisterModel request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _authService.Register(request);

            return Ok(response);
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new ResponseModel("error", 400, "Email is required.", null));
            }

            var response = await _authService.ResetPassword(model.Email);
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/verify-code (for phone)
        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromQuery] long userId, [FromQuery] string code)
        {
            // Validate the provided verification code
            // Mark the account as verified if the code is correct
            return Ok(new { Message = "Code verified successfully." });
        }

        // POST: api/auth/url-avatar (test)
        [HttpPost("url-avatar")]
        [Consumes("multipart/form-data")] // Tell Swagger this is a file upload endpoint
        public async Task<ActionResult> UploadAvatar( IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ResponseModel("error", 400, "No file uploaded.", null));
            }

            var response = await _authService.GetUrlAvatar(file);
            return Ok(response);
        }
    }
}
