using UserService.Application.DTOs.Auths;

namespace UserService.Application.Commands.Handlers;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.ResponseModel;

public class SendPhoneOtpCommandHandler : IRequestHandler<SendPhoneOtpCommand, ResponseModel>
{
    private readonly IConfiguration _configuration;

    public SendPhoneOtpCommandHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ResponseModel> Handle(SendPhoneOtpCommand? requestModel, CancellationToken cancellationToken)
    {
        try
        {
            if (requestModel == null || string.IsNullOrEmpty(requestModel.RequestModel?.PhoneNumber))
            {
                return new ResponseModel("error", 400, "Phone number is required.", null);
            }

            if (!Regex.IsMatch(requestModel.RequestModel.PhoneNumber, @"^\+?[1-9]\d{1,14}$")) // E.164 format check
            {
                return new ResponseModel("error", 400, "Invalid phone number format.", null);
            }

            var firebaseApiKey = _configuration["Firebase:ApiKey"];

            // Firebase REST API for sending OTP
            var sendOtpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendVerificationCode?key={firebaseApiKey}";

            var payload = new
            {
                phoneNumber = requestModel.RequestModel.PhoneNumber,
                recaptchaToken = requestModel.RequestModel.RecaptchaToken
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(sendOtpUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Failed to send OTP.", responseBody);
            }

            // Extract VerificationId (sessionInfo) from Firebase response
            var result = JsonSerializer.Deserialize<FirebaseSendOtpResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (string.IsNullOrEmpty(result?.SessionInfo))
            {
                return new ResponseModel("error", 500, "Failed to generate verification ID.", null);
            }

            return new ResponseModel("success", 200, "OTP sent successfully.", new
            {
                VerificationId = result.SessionInfo
            });
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }
}

