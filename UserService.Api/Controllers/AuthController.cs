using System.Security.Claims;
using Firebase.Auth.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using MediatR;
using UserService.Application.Commands;
using UserService.Application.Queries;

namespace UserService.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
            return Ok(new { message = "Authenticated!", userId, email });
        }

        // ✅ Public API - No authorization required
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestModel? loginRequest)
        {
            if (loginRequest == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new LoginCommand() { LoginModel = loginRequest });
            return StatusCode(response.StatusCode, response);
        }

        // ✅ Public API - Register a new user
        [HttpPost("register-user")]
        public async Task<ActionResult> RegisterUser([FromForm] UserRegisterModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new UserRegisterCommand { RegisterModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // Public API - Register a new user
        [HttpPost("register-shop")]
        public async Task<ActionResult> RegisterShop([FromForm] ShopOwnerRegisterModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new ShopRegisterCommand { RegisterModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // Any authenticated user can reset password
        [Authorize]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequest? model)
        {
            if (model == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new ResetPasswordCommand { Email = model.Email });
            return StatusCode(response.StatusCode, response);
        }

        // Public API - No authorization required
        [HttpPost("send-verify-email")]
        public async Task<IActionResult> SendVerifyEmail([FromForm] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ResponseModel("error", 400, "Email is required.", null));
            }

            var response = await _mediator.Send(new SendEmailVerifyCommand { Email = email });
            return StatusCode(response.StatusCode, response);
        }

        // Public API - No authorization required
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendPhoneOtpRequestModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new SendPhoneOtpCommand { RequestModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // Public API - No authorization required
        [HttpPost("verify-otp-login")]
        public async Task<IActionResult> VerifyOtpForLogin([FromBody] VerifyPhoneOtpRequestModel? requestModel)
        {
            if (requestModel == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new VerifyPhoneOtpCommand { RequestModel = requestModel });
            return StatusCode(response.StatusCode, response);
        }

        // Restricted to authenticated users only
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromForm] RefreshTokenRequestModel request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new ResponseModel("error", 400, "User Id is Required", null));

            var response = await _mediator.Send(new LogoutCommand { RequestModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // Restricted to authenticated users only
        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] RefreshTokenRequestModel request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new ResponseModel("error", 400, "RefreshToken is Required", null));

            var response = await _mediator.Send(new RefreshTokenCommand { RegisterModel = request });
            return StatusCode(response.StatusCode, response);
        }
    }
}
