using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Services;

namespace UserService.Application.Commands.Handlers;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.ResponseModel;

public class SendEmailVerifyCommandHandler : IRequestHandler<SendEmailVerifyCommand, ResponseModel>
{
    private readonly IConfiguration _configuration;
    private readonly IAuthRepository _authRepository;

    public SendEmailVerifyCommandHandler(IConfiguration configuration, IAuthRepository authRepository)
    {
        _configuration = configuration;
        _authRepository = authRepository;
    }

    public async Task<ResponseModel> Handle(SendEmailVerifyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return new ResponseModel("error", 400, "Email is required.", null);
            }

            UserRecord? user = null;
            bool userExists = true;

            // Try to fetch user from Firebase
            try
            {
                user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(request.Email, cancellationToken);
            }
            catch (FirebaseAuthException)
            {
                userExists = false; // User does not exist
            }

            // If user does not exist, create a new one with a temporary password
            if (!userExists)
            {
                try
                {
                    var createUserArgs = new UserRecordArgs
                    {
                        Email = request.Email,
                        Password = "TempPassword123!", // Temporary password
                        EmailVerified = false,
                        Disabled = false
                    };

                    user = await FirebaseAuth.DefaultInstance.CreateUserAsync(createUserArgs, cancellationToken);
                }
                catch (FirebaseAuthException createEx)
                {
                    return new ResponseModel("error", 500, "Failed to create user.", createEx.Message);
                }
                
                
                
            }
            // If email is already verified, no need to send another verification email
            if (user is { EmailVerified: true })
            {
                return new ResponseModel("success", 200, "Email is already verified.", null);
            }
            var firebaseApiKey = _configuration["Firebase:ApiKey"];

            
            var verifyEmailUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";
            var userResponse = await _authRepository.HandleEmailPasswordSignInWithTokensAsync(request.Email, "TempPassword123!");
            
            // Payload for sending verification email
            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken = userResponse.IdToken,
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(verifyEmailUrl, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Failed to send verification email.", responseBody);
            }

            return new ResponseModel("success", 200, "Verification email sent successfully. Please check your email.", 
                user);
        }
        catch (FirebaseAuthException ex)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", ex.Message);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }
}

