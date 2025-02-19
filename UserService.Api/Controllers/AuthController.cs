using System.ComponentModel.DataAnnotations;
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
        
        // POST: api/auth/send-verify-email
        [HttpPost("send-verify-email")]
        public async Task<IActionResult> SendVerifyEmail([FromBody] string? email)
        {
            if (email == null)
            {
                return BadRequest(new ResponseModel("error", 400, "Email is required.", null));
            }

            var response = await _authService.SendEmailVerify(email);
            return StatusCode(response.StatusCode, response);
        }
        
        // POST: api/auth/send-otp
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendPhoneOtpRequestModel loginOtpRequest)
        {
            if (string.IsNullOrEmpty(loginOtpRequest.PhoneNumber))
            {
                return BadRequest(new ResponseModel("error", 400, "PhoneNumber is required.", null));
            }
            var response = await _authService.SendPhoneOtp(loginOtpRequest);
            return StatusCode(response.StatusCode, response);
        }
        
        // POST: api/auth/verify-otp-login
        [HttpPost("verify-otp-login")]
        public async Task<IActionResult> VerifyOtpForLogin([FromBody] VerifyPhoneOtpRequestModel? requestModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel("error", 400, "IdToken is required.", null));
            }
            var response = await _authService.VerifyPhoneOtp(requestModel);
            return StatusCode(response.StatusCode, response);
            
        }

        // POST: api/auth/verify-otp-link-credential-user
        [HttpPost("verify-otp-link-credential-user")]
        public async Task<IActionResult> VerifyOtpForLinkWithCredentialUser([FromBody] VerifyPhoneNumberLinkRequestModel? requestModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel("error", 400, "FirebaseId is required.", null));
            }
            var response = await _authService.VerifyPhoneNumberLinkWithCredential(requestModel);
            return StatusCode(response.StatusCode, response);
            
        }

    }
}
