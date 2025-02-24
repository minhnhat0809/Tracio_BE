namespace UserService.Application.Commands.Handlers;

using FirebaseAdmin.Auth;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.Sessions;
using UserService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Domain.Entities;
using UserService.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ResponseModel>
{
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;
    private readonly IUnitOfWork _repository;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IUnitOfWork repository,
        IMapper mapper,
        IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (request.LoginModel == null)
            return new ResponseModel("error", 400, "Invalid request data.", null);

        (User? user, string idToken, string refreshToken) signInResult;
        try
        {
            // Google or Email/Password
            if (!string.IsNullOrEmpty(request.LoginModel.IdToken))
            {
                signInResult = await _firebaseAuthenticationRepository.HandleGoogleSignInWithTokensAsync(request.LoginModel.IdToken);
                if (request.LoginModel.RefreshToken == null) 
                    return new ResponseModel("error", 400, "RefreshToken is Required", null);
                signInResult.refreshToken = request.LoginModel.RefreshToken;
            }
            else if (!string.IsNullOrEmpty(request.LoginModel.Email) && !string.IsNullOrEmpty(request.LoginModel.Password))
            {
                signInResult = await _firebaseAuthenticationRepository.HandleEmailPasswordSignInWithTokensAsync(request.LoginModel.Email, request.LoginModel.Password);
            }
            else
            {
                return new ResponseModel("error", 400, "Invalid login request. Provide either IdToken or Email/Password.", null);
            }
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, $"Authentication error: {ex.Message}", null);
        }

        if (signInResult.user == null)
            return new ResponseModel("error", 404, "User not found in the system.", null);

        // ✅ **Create a new session**
        try
        {
            var session = new UserSession
            {
                UserId = signInResult.user.UserId,
                AccessToken = signInResult.idToken,
                RefreshToken = signInResult.refreshToken,
                ExpiresAt = DateTime.Now.AddHours(1),
                CreatedAt = DateTime.Now
            };
            await _repository.UserSessionRepository.CreateAsync(session);
            
            var responseModel = _mapper.Map<LoginViewModel>(signInResult.user);
            responseModel.Session = _mapper.Map<SessionViewModel>(session);

            return new ResponseModel("success", 200, "Login successful.", responseModel);
        }
        catch (Exception dbEx)
        {
            return new ResponseModel("error", 500, "Failed to create user session.", dbEx.Message);
        }
    }
}
