using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.Sessions;
using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Domain.Entities;

public class LoginQueryHandler : IRequestHandler<LoginQuery, ResponseModel>
{
    private readonly IAuthRepository _authRepository;
    private readonly IUnitOfWork _repository;
    private readonly IMapper _mapper;

    public LoginQueryHandler(IAuthRepository authRepository, IUnitOfWork repository, IMapper mapper)
    {
        _authRepository = authRepository;
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var login = request.LoginModel;
        if (login == null)
        {
            return new ResponseModel("error", 400, "Invalid request data.", null);
        }

        User user;
        string firebaseIdToken;
        string firebaseRefreshToken;

        if (!string.IsNullOrEmpty(login.IdToken))
        {
            var googleSignInResult = await _authRepository.HandleGoogleSignInWithTokensAsync(login.IdToken);
            if (googleSignInResult.User == null)
            {
                return new ResponseModel("error", 401, "Google authentication failed.", null);
            }
            user = googleSignInResult.User;
            firebaseIdToken = googleSignInResult.IdToken;
            firebaseRefreshToken = "LoginWithGoogleNotIncludingRefreshToken";
        }
        else if (!string.IsNullOrEmpty(login.Email) && !string.IsNullOrEmpty(login.Password))
        {
            var emailPasswordSignInResult = await _authRepository.HandleEmailPasswordSignInWithTokensAsync(login.Email, login.Password);
            if (emailPasswordSignInResult.User == null)
            {
                return new ResponseModel("error", 401, "Invalid email or password.", null);
            }
            user = emailPasswordSignInResult.User;
            firebaseIdToken = emailPasswordSignInResult.IdToken;
            firebaseRefreshToken = emailPasswordSignInResult.RefreshToken;
        }
        else
        {
            return new ResponseModel("error", 400, "Invalid login request.", null);
        }

        var session = new UserSession
        {
            UserId = user.UserId,
            AccessToken = firebaseIdToken,
            RefreshToken = firebaseRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.UserSessionRepository.CreateAsync(session);

        var responseModel = _mapper.Map<LoginViewModel>(user);
        responseModel.Session = _mapper.Map<SessionViewModel>(session);

        return new ResponseModel("success", 200, "Login successful.", responseModel);
    }
}
