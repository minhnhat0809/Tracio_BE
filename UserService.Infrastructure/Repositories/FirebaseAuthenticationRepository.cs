using System.Text;
using System.Text.Json;
using AutoMapper;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Sessions;
using UserService.Application.Interfaces;
using FirebaseAuthClient = Firebase.Auth.FirebaseAuthClient;
using FirebaseAuthConfig = Firebase.Auth.FirebaseAuthConfig;
using FirebaseAuthException = FirebaseAdmin.Auth.FirebaseAuthException;
using User = UserService.Domain.Entities.User;

namespace UserService.Infrastructure.Repositories;



public class FirebaseAuthenticationRepository : IFirebaseAuthenticationRepository
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _repository;
    public FirebaseAuthenticationRepository(IConfiguration configuration, IUnitOfWork repository)
    {
        _configuration = configuration;
        _repository = repository;
    }

    public async Task<UserRecord> GetFirebaseUserByUidAsync(string uid, CancellationToken cancellationToken)
    {
        return await FirebaseAuth.DefaultInstance.GetUserAsync(uid, cancellationToken);
    }
    
    public async Task<UserRecord> GetFirebaseUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email, cancellationToken);
    }

    public async Task SetCustomClaimsAsync(string firebaseId, Dictionary<string, object> claims, CancellationToken cancellationToken)
    {
        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(firebaseId, claims, cancellationToken);
    }
    
   
    public async Task RevokeRefreshTokensAsync(string firebaseId, CancellationToken cancellationToken)
    {
        await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(firebaseId, cancellationToken);
    }

    public async Task<string> SendPasswordResetEmailAsync(string email, string apiKey, CancellationToken cancellationToken)
    {
        var resetUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";
        var payload = JsonSerializer.Serialize(new { requestType = "PASSWORD_RESET", email });
        using var client = new HttpClient();
        var response = await client.PostAsync(resetUrl, new StringContent(payload, Encoding.UTF8, "application/json"), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> SendEmailVerificationAsync(string email, string tempPassword, string apiKey, CancellationToken cancellationToken)
    {
        var authProvider = new FirebaseAuthClient(new FirebaseAuthConfig
        {
            ApiKey = apiKey,
            AuthDomain = "tracio-cbd26.firebaseapp.com",
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        });
        UserCredential authResult;
        try
        {
            authResult = await authProvider.SignInWithEmailAndPasswordAsync(email, tempPassword);
        }
        catch (FirebaseAuthException)
        {
            return "Authentication failed: Invalid email or password.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            return "Unexpected error occurred during authentication.";
        }

        var payload = JsonSerializer.Serialize(new { requestType = "VERIFY_EMAIL", idToken = authResult.User.Credential.IdToken});
        var verifyUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={apiKey}";

        using var client = new HttpClient();
        var response = await client.PostAsync(verifyUrl, new StringContent(payload, Encoding.UTF8, "application/json"), cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return $"Failed to send email verification: {response.StatusCode} - {responseBody}";
        }

        return "Verification email sent successfully.";
    }


    public async Task<FirebaseSendOtpResponse?> SendPhoneOtpAsync(string phoneNumber, string recaptchaToken, string apiKey, CancellationToken cancellationToken)
    {
        var sendOtpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendVerificationCode?key={apiKey}";
        var payload = JsonSerializer.Serialize(new { phoneNumber, recaptchaToken });
        using var client = new HttpClient();
        var response = await client.PostAsync(sendOtpUrl, new StringContent(payload, Encoding.UTF8, "application/json"), cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<FirebaseSendOtpResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<FirebaseAuthResponse?> VerifyPhoneOtpAsync(string verificationId, string otpCode, string apiKey, CancellationToken cancellationToken)
    {
        var verifyOtpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={apiKey}";
        var payload = JsonSerializer.Serialize(new { sessionInfo = verificationId, code = otpCode });
        using var client = new HttpClient();
        var response = await client.PostAsync(verifyOtpUrl, new StringContent(payload, Encoding.UTF8, "application/json"), cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<FirebaseAuthResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    
    public async Task<ResponseModel?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return new ResponseModel("error", 400, "Refresh token cannot be empty.", null);
        }

        var firebaseApiKey = _configuration["Firebase:ApiKey"];
        var refreshTokenUrl = $"https://securetoken.googleapis.com/v1/token?key={firebaseApiKey}";

        var formData = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

        using var client = new HttpClient();
        var content = new FormUrlEncodedContent(formData);

        try
        {
            var response = await client.PostAsync(refreshTokenUrl, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (responseBody.Contains("TOKEN_EXPIRED"))
                {
                    return new ResponseModel("error", 401, "Refresh token expired. Please log in again.", null);
                }

                return new ResponseModel("error", (int)response.StatusCode, "Firebase token refresh failed.", responseBody);
            }

            var refreshResponse = JsonSerializer.Deserialize<FirebaseRefreshTokenResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (refreshResponse == null || string.IsNullOrEmpty(refreshResponse.RefreshToken))
            {
                return new ResponseModel("error", 500, "Invalid token response from Firebase.", null);
            }

            return new ResponseModel("success", 200, "Token refreshed successfully.", new FirebaseAuthResponse
            {
                IdToken = refreshResponse.IdToken,
                RefreshToken = refreshResponse.RefreshToken,
                ExpiresIn = refreshResponse.ExpiresIn,
            });
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred during token refresh.", ex.Message);
        }
    }
    public async Task<(User? User, string IdToken, string RefreshToken)> HandleGoogleSignInWithTokensAsync(string idToken)
    {
        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string uid = decodedToken.Uid;
            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
            if (firebaseUser == null)
                return (null, string.Empty, string.Empty);

            var user = await _repository.UserRepository.GetUserByPropertyAsync(firebaseUser.Email);
            // Revoke Firebase token (only if user exists)
            if (user != null)
            {
                await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(user.FirebaseId);

                // Revoke database tokens (delete sessions)
                await _repository.UserSessionRepository.RevokeAllSessionsForUser(user.UserId);
                return (user, idToken, string.Empty);
            }
            return (null, string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return (null, string.Empty, string.Empty);
        }
    }

    
    public async Task<(User? User, string IdToken, string RefreshToken)> HandleEmailPasswordSignInWithTokensAsync(string email, string password)
    {
        var firebaseApiKey = _configuration["Firebase:ApiKey"];
        var authProvider = new FirebaseAuthClient(new FirebaseAuthConfig
        {
            ApiKey = firebaseApiKey,
            AuthDomain = "tracio-cbd26.firebaseapp.com",
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        });
        var user = await _repository.UserRepository.GetUserByPropertyAsync(email);
        if (user == null)
        {
            return (null, string.Empty, string.Empty);
        }

        // Revoke Firebase token (only if user exists)
        await FirebaseAuth.DefaultInstance.RevokeRefreshTokensAsync(user.FirebaseId);
        
        // Revoke database tokens (delete sessions)
        await _repository.UserSessionRepository.RevokeAllSessionsForUser(user.UserId);
        try
        {
            // Generate new access token and refresh token
            var authResult = await authProvider.SignInWithEmailAndPasswordAsync(email, password);

            return (user, authResult.User.Credential.IdToken, authResult.User.Credential.RefreshToken);
        }
        catch (Firebase.Auth.FirebaseAuthException)
        {
            return (null, string.Empty, string.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
            return (null, string.Empty, string.Empty);
        }
    }
  


}
