using Google.Apis.Http;
using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using FirebaseAdmin.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.ResponseModel;
using System.ComponentModel.DataAnnotations;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResponseModel>
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _repository;

    public ResetPasswordCommandHandler(IConfiguration configuration, IUnitOfWork repository)
    {
        _configuration = configuration;
        _repository = repository;
    }

    public async Task<ResponseModel> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate email input
            if (string.IsNullOrEmpty(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
            {
                return new ResponseModel("error", 400, "Invalid email format.", null);
            }

            // Get Firebase User
            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(request.Email, cancellationToken);
                if (firebaseUser == null)
                {
                    return new ResponseModel("error", 404, "User not found in Firebase.", null);
                }
                // Check Verify
                if (!firebaseUser.EmailVerified)
                {
                    return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
                }
            }
            catch (FirebaseAuthException firebaseEx)
            {
                return new ResponseModel("error", 500, "Firebase authentication error.", firebaseEx.Message);
            }

            // Check if user exists in local database
            var user = await _repository.UserRepository.GetUserByPropertyAsync(request.Email);
            if (user == null)
            {
                return new ResponseModel("error", 404, "User not found in the system.", null);
            }

            // Get Firebase API Key
            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
            }

            // Firebase Password Reset URL
            var resetPasswordUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";

            // Payload for Firebase password reset request
            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email = request.Email,
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsync(resetPasswordUrl, content,cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel("error", (int)response.StatusCode, "Failed to send password reset email.", responseBody);
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new ResponseModel("error", 500, "Network error while sending reset email.", httpEx.Message);
            }

            return new ResponseModel("success", 200, "Password reset email sent successfully.", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ResetPassword] Unexpected error: {ex.Message}");
            return new ResponseModel("error", 500, "An unexpected server error occurred.", "Internal Server Error.");
        }
    }
}

