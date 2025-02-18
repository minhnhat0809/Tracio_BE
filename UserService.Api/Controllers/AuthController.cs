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
        public async Task<ActionResult> Login([FromBody] LoginRequestModel? loginRequest) // ✅ Expect JSON request body
        {
            if (loginRequest == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _authService.Login(loginRequest);
            return Ok(response);
        }

        // POST: api/auth/register-user (Register a new user)
        [HttpPost("register-user")]
        public async Task<ActionResult> RegisterUser([FromForm] UserRegisterModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _authService.UserRegister(request);

            return Ok(response);
        }
        
        // POST: api/auth/register-shop (Register a new user)
        [HttpPost("register-shop")]
        public async Task<ActionResult> RegisterShop([FromForm] ShopOwnerRegisterModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _authService.ShopRegister(request);

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
        
        

        
    }
}
