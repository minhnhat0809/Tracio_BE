using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.ResponseModel;
using System.ComponentModel.DataAnnotations;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResponseModel>
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _repository;
    private readonly IFirebaseAuthenticationRepository _firebaseAuthenticationRepository;
    public ResetPasswordCommandHandler(IConfiguration configuration, IUnitOfWork repository, IFirebaseAuthenticationRepository firebaseAuthenticationRepository)
    {
        _configuration = configuration;
        _repository = repository;
        _firebaseAuthenticationRepository = firebaseAuthenticationRepository;
    }

    public async Task<ResponseModel> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Email) || !new EmailAddressAttribute().IsValid(request.Email))
            return new ResponseModel("error", 400, "Invalid email format.", null);

        try
        {
            var firebaseUser = await _firebaseAuthenticationRepository.GetFirebaseUserByEmailAsync(request.Email, cancellationToken);
            if (!firebaseUser.EmailVerified)
                return new ResponseModel("error", 404, "User not found or email not verified in Firebase.", null);
        }
        catch (FirebaseAuthException fbaEx)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", fbaEx.Message);
        }

        var user = await _repository.UserRepository.GetUserByPropertyAsync(request.Email);
        if (user == null)
            return new ResponseModel("error", 404, "User not found in the system.", null);

        var firebaseApiKey = _configuration["Firebase:ApiKey"];
        if (string.IsNullOrEmpty(firebaseApiKey))
            return new ResponseModel("error", 500, "Firebase API Key is missing!", null);

        try
        {
            await _firebaseAuthenticationRepository.SendEmailResetPasswordAsync(request.Email, firebaseApiKey, cancellationToken);
            return new ResponseModel("success", 200, "Password reset email sent successfully.", null);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected server error occurred.", ex.Message);
        }
    }
}

