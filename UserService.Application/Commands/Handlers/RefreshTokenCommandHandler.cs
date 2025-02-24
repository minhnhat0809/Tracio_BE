using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Sessions;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Commands.Handlers;
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

    public async Task<ResponseModel> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RegisterModel?.RefreshToken))
        {
            return new ResponseModel("error", 400, "Refresh Token is required.", null);
        }

        // Retrieve the existing session.
        var session = await _unitOfWork.UserSessionRepository.GetSessionByRefreshToken(request.RegisterModel.RefreshToken);
        if (session == null)
        {
            return new ResponseModel("error", 401, "Unauthorized: Invalid or expired refresh token.", null);
        }

        // Refresh the tokens using Firebase.
        var refreshResponse = await _firebaseAuthenticationRepository.RefreshTokenAsync(request.RegisterModel.RefreshToken, cancellationToken);
        if (refreshResponse.Status != "success")
        {
            return refreshResponse;
        }

        var firebaseTokens = refreshResponse.Result as FirebaseAuthResponse;
        if (firebaseTokens == null)
        {
            return new ResponseModel("error", 500, "Unexpected token processing error.", null);
        }

        // Revoke the old refresh token
        await _unitOfWork.UserSessionRepository.RevokeSessionByRefreshToken(request.RegisterModel.RefreshToken);

        // Update the session with the new tokens.
        session.AccessToken = firebaseTokens.IdToken;
        session.RefreshToken = firebaseTokens.RefreshToken;
        session.ExpiresAt = DateTime.UtcNow.AddSeconds(int.Parse(firebaseTokens.ExpiresIn));
        session.CreatedAt = DateTime.UtcNow;
        await _unitOfWork.UserSessionRepository.UpdateAsync(session);

        // Retrieve user details.
        var user = await _unitOfWork.UserRepository.GetByIdAsync(session.UserId);
        if (user == null)
        {
            return new ResponseModel("error", 404, "User not found.", null);
        }

        var responseModel = _mapper.Map<LoginViewModel>(user);
        responseModel.Session = _mapper.Map<SessionViewModel>(session);

        return new ResponseModel("success", 200, "Token refreshed successfully.", responseModel);
    }
}
