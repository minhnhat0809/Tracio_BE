using UserService.Application.Interfaces;

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
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;
    public SendPhoneOtpCommandHandler(IConfiguration configuration, IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _configuration = configuration;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

    public async Task<ResponseModel> Handle(SendPhoneOtpCommand? requestModel, CancellationToken cancellationToken)
    {
        if (requestModel == null || string.IsNullOrEmpty(requestModel.RequestModel?.PhoneNumber))
        {
            return new ResponseModel("error", 400, "Phone number is required.", null);
        }

        // Validate phone number against E.164 format.
        if (!Regex.IsMatch(requestModel.RequestModel.PhoneNumber, @"^\+?[1-9]\d{1,14}$"))
        {
            return new ResponseModel("error", 400, "Invalid phone number format.", null);
        }

        var firebaseApiKey = _configuration["Firebase:ApiKey"];
        if (string.IsNullOrEmpty(firebaseApiKey))
        {
            return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
        }

        try
        {
            var otpResponse = await _firebaseAuthenticationRepository.SendPhoneOtpAsync(
                requestModel.RequestModel.PhoneNumber, 
                requestModel.RequestModel.RecaptchaToken, 
                firebaseApiKey, 
                cancellationToken);
            if (string.IsNullOrEmpty(otpResponse?.SessionInfo))
            {
                return new ResponseModel("error", 500, "Failed to generate verification ID.", null);
            }

            return new ResponseModel("success", 200, "OTP sent successfully.", new { VerificationId = otpResponse.SessionInfo });
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }
}

