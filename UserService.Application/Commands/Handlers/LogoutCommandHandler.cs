using UserService.Application.DTOs.ResponseModel;
using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _unitOfWork = unitOfWork;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

    public async Task<ResponseModel> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RequestModel.RefreshToken))
        {
            return new ResponseModel("error", 400, "Refresh token is required.", null);
        }
            
        // Retrieve the session using the provided refresh token.
        var session = await _unitOfWork.UserSessionRepository.GetSessionByRefreshToken(request.RequestModel.RefreshToken);
        if (session == null)
        {
            return new ResponseModel("error", 404, "Session not found.", null);
        }
            
        // Revoke all Firebase tokens 
        var user = await _unitOfWork.UserRepository.GetByIdAsync(session.UserId);
        if (user != null)
        {
            await _firebaseAuthenticationRepository.RevokeRefreshTokensAsync(user.FirebaseId, cancellationToken);
        }
            
        // Revoke (delete) the local session record.
        var revoked = await _unitOfWork.UserSessionRepository.RevokeSessionByRefreshToken(request.RequestModel.RefreshToken);
        if (revoked == null)
        {
            return new ResponseModel("error", 500, "Failed to revoke session.", null);
        }

        return new ResponseModel("success", 200, "Logout successful.", null);
    }
}