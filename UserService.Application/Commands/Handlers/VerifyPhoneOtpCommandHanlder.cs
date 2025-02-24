using AutoMapper;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.Sessions;
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
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;
    public VerifyPhoneOtpCommandHandler(IUnitOfWork repository, IConfiguration configuration, IMapper mapper, IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _repository = repository;
        _configuration = configuration;
        _mapper = mapper;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

    public async Task<ResponseModel> Handle(VerifyPhoneOtpCommand? requestModel, CancellationToken cancellationToken)
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

       try
       {
           // Delegate OTP verification to FirebaseService.
           var authResponse = await _firebaseAuthenticationRepository.VerifyPhoneOtpAsync(
               requestModel.RequestModel.VerificationId, 
               requestModel.RequestModel.OtpCode, 
               firebaseApiKey,
               cancellationToken);
           
           if (string.IsNullOrEmpty(authResponse?.IdToken))
           {
               return new ResponseModel("error", 401, "Authentication failed. No token received.", null);
           }

           // Verify the token and retrieve the UID.
           var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authResponse.IdToken, cancellationToken);
           var uid = decodedToken.Uid;

           // Retrieve the Firebase user.
           var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid, cancellationToken);
           if (firebaseUser == null)
           {
               return new ResponseModel("error", 404, "User not found.", null);
           }

           // Retrieve the user from your local database.
           var user = await _repository.UserRepository.GetUserByPropertyAsync(uid);
           if (user == null)
           {
               return new ResponseModel("error", 404, "User not found in Database", null);
           }

           // Create a session for the user.
           var session = new UserSession
           {
               UserId = user.UserId,
               AccessToken = authResponse.IdToken,
               RefreshToken = authResponse.RefreshToken,
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
