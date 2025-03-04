using System.ComponentModel.DataAnnotations;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;

    public SendEmailVerifyCommandHandler(IConfiguration configuration, IUnitOfWork unitOfWork, IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

   public async Task<ResponseModel> Handle(SendEmailVerifyCommand request, CancellationToken cancellationToken)
{
    if (string.IsNullOrEmpty(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
    {
        return new ResponseModel("error", 400, "Email is required.", null);
    }

    UserRecord? userRecord = null;
    bool userExists = true;

    // Try to get the user from the local repository
    var user = await _unitOfWork.UserRepository.GetUserByPropertyAsync(request.Email);
    if (user == null)
    {
        // Local user not found, so we'll create a new Firebase user.
        userExists = false;
    }
    else
    {
        try
        {
            // Try to get the Firebase user using the local user's FirebaseId.
            userRecord = await _firebaseAuthenticationRepository.GetFirebaseUserByUidAsync(user.FirebaseId, cancellationToken);
        }
        catch (FirebaseAuthException)
        {
            // Firebase user not found, so mark as not existing.
            userExists = false;
        }
    }

    // If user does not exist in Firebase (or locally), create a new Firebase user with a temporary password.
    if (!userExists)
    {
        try
        {
            var createUserArgs = new UserRecordArgs
            {
                Email = request.Email,
                Password = "TempPassword123!",
                EmailVerified = false,
                Disabled = false
            };
            userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(createUserArgs, cancellationToken);
        }
        catch (FirebaseAuthException createEx)
        {
            return new ResponseModel("error", 500, "Failed to create user.", createEx.Message);
        }
    }

    // If the user's email is already verified, return a success response.
    if (userRecord != null && userRecord.EmailVerified)
    {
        return new ResponseModel("success", 200, "Email is already verified.", null);
    }

    var firebaseApiKey = _configuration["Firebase:ApiKey"];
    if (string.IsNullOrEmpty(firebaseApiKey))
    {
        return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
    }

    try
    {
        // Delegate sending the email verification to the FirebaseAuthenticationRepository.
        var verificationResponse = await _firebaseAuthenticationRepository.SendEmailVerifyAccountAsync(
            request.Email, "TempPassword123!", firebaseApiKey, cancellationToken);

        if (string.IsNullOrEmpty(verificationResponse))
        {
            return new ResponseModel("error", 500, "Verification email was not sent. Please try again.", null);
        }

        return new ResponseModel("success", 200, "Verification email sent successfully. Please check your email.", userRecord);
    }
    catch (HttpRequestException httpEx)
    {
        return new ResponseModel("error", 500, "Failed to send verification email due to network issues.", httpEx.Message);
    }
    catch (Exception ex)
    {
        return new ResponseModel("error", 500, "An error occurred while sending the verification email.", ex.Message);
    }
}

}

