using Firebase.Auth.Requests;
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

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequestModel? loginRequest)
        {
            if (loginRequest == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new LoginQuery { LoginModel = loginRequest });
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/register-user (Register a new user)
        [HttpPost("register-user")]
        public async Task<ActionResult> RegisterUser([FromForm] UserRegisterModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new UserRegisterCommand { RegisterModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/register-shop (Register a new shop owner)
        [HttpPost("register-shop")]
        public async Task<ActionResult> RegisterShop([FromForm] ShopOwnerRegisterModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new ShopRegisterCommand { RegisterModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest? model)
        {
            if (model == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));


            var response = await _mediator.Send(new ResetPasswordCommand { Email = model.Email });
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/send-verify-email
        [HttpPost("send-verify-email")]
        public async Task<IActionResult> SendVerifyEmail([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ResponseModel("error", 400, "Email is required.", null));
            }

            var response = await _mediator.Send(new SendEmailVerifyCommand { Email = email });
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/send-otp
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendPhoneOtpRequestModel? request)
        {
            if (request == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));

            var response = await _mediator.Send(new SendPhoneOtpCommand { RequestModel = request });
            return StatusCode(response.StatusCode, response);
        }

        // POST: api/auth/verify-otp-login
        [HttpPost("verify-otp-login")]
        public async Task<IActionResult> VerifyOtpForLogin([FromBody] VerifyPhoneOtpRequestModel? requestModel)
        {
            if (requestModel == null)
                return BadRequest(new ResponseModel("error", 400, "Invalid request data.", null));


            var response = await _mediator.Send(new VerifyPhoneOtpCommand { RequestModel = requestModel });
            return StatusCode(response.StatusCode, response);
        }
    }
}
