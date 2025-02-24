using FirebaseAdmin.Auth;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IFirebaseAuthenticationRepository
{
    Task<UserRecord> GetFirebaseUserByUidAsync(string uid, CancellationToken cancellationToken);
    Task<UserRecord> GetFirebaseUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task SetCustomClaimsAsync(string firebaseId, Dictionary<string, object> claims, CancellationToken cancellationToken);
    Task<string> SendPasswordResetEmailAsync(string email, string apiKey, CancellationToken cancellationToken);
    Task<string> SendEmailVerificationAsync(string email, string tempPassword, string apiKey, CancellationToken cancellationToken);
    Task<FirebaseSendOtpResponse?> SendPhoneOtpAsync(string phoneNumber, string recaptchaToken, string apiKey, CancellationToken cancellationToken);
    Task<FirebaseAuthResponse?> VerifyPhoneOtpAsync(string verificationId, string otpCode, string apiKey, CancellationToken cancellationToken);
    Task<ResponseModel?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeRefreshTokensAsync(string firebaseId, CancellationToken cancellationToken);
    Task<(User? User, string IdToken, string RefreshToken)> HandleGoogleSignInWithTokensAsync(string idToken);
    Task<(User? User, string IdToken, string RefreshToken)> HandleEmailPasswordSignInWithTokensAsync(string email, string password);
}