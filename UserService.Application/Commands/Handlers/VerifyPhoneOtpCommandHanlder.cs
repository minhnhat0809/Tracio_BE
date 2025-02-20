using AutoMapper;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;

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
using UserService.Domain.Entities;

public class VerifyPhoneOtpCommandHandler : IRequestHandler<VerifyPhoneOtpCommand, ResponseModel>
{
    private readonly IUnitOfWork _repository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    
    public VerifyPhoneOtpCommandHandler(IUnitOfWork repository, IConfiguration configuration, IMapper mapper)
    {
        _repository = repository;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(VerifyPhoneOtpCommand? requestModel, CancellationToken cancellationToken)
    {
        try
        {
            if (requestModel == null || string.IsNullOrEmpty(requestModel.RequestModel?.VerificationId) || string.IsNullOrEmpty(requestModel.RequestModel.OtpCode))
            {
                return new ResponseModel("error", 400, "Verification ID and OTP are required.", null);
            }

            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
            }

            // Firebase API URL to verify OTP
            var verifyOtpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={firebaseApiKey}";

            // Payload to verify OTP
            var payload = new
            {
                sessionInfo = requestModel.RequestModel.VerificationId, // The verification ID from frontend
                code = requestModel.RequestModel.OtpCode // The OTP code entered by the user
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(verifyOtpUrl, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Invalid OTP.", responseBody);
            }

            // Extract idToken from response
            var result = JsonSerializer.Deserialize<FirebaseAuthResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (string.IsNullOrEmpty(result?.IdToken))
            {
                return new ResponseModel("error", 401, "Authentication failed. No token received.", null);
            }

            // Get user info from Firebase
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(result.IdToken, cancellationToken);
            var uid = decodedToken.Uid;

            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid, cancellationToken);
            if (firebaseUser == null)
            {
                return new ResponseModel("error", 404, "User not found.", null);
            }

            // Check if user exists in the database
            var user = await _repository.UserRepository.GetUserByPropertyAsync(uid);
            if (user == null)
            {
                return new ResponseModel("error", 404, "User not found in Database", null);
            }

            // Create session for the user
            var session = new UserSession
            {
                UserId = user.UserId,
                AccessToken = result.IdToken,
                RefreshToken = result.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                CreatedAt = DateTime.UtcNow
            };

            await _repository.UserSessionRepository.CreateAsync(session);


            return new ResponseModel("success", 200, "Phone login successful.", _mapper.Map<UserViewModel>(user));
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }
}
