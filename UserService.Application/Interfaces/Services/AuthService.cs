/*
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using FirebaseAdmin.Auth;
using Google.Apis.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Sessions;
using UserService.Application.DTOs.Users;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ResponseModel> Login( LoginRequestModel loginModel);
    Task<ResponseModel> UserRegister(UserRegisterModel registerModel);
    Task<ResponseModel> ShopRegister(ShopOwnerRegisterModel registerModel);
   
    Task<ResponseModel> ResetPassword(string email);
    Task<ResponseModel> SendEmailVerify(string? email);
    Task<ResponseModel> SendPhoneOtp(SendPhoneOtpRequestModel? loginModel);
    Task<ResponseModel> VerifyPhoneOtp(VerifyPhoneOtpRequestModel? email);

}
public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IFirebaseAuthenticationService _firebaseAuthenticationService;
    private readonly IFirebaseStorageRepository _firebaseStorageRepository;
    private readonly IUnitOfWork _repository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    
    public AuthService(
        IAuthRepository authRepository,
        IUnitOfWork repository,
        IMapper mapper,
        IConfiguration configuration,
        IFirebaseAuthenticationService firebaseAuthenticationService, IFirebaseStorageRepository firebaseStorageRepository)
    {
        _authRepository = authRepository;
        _repository = repository;
        _mapper = mapper;
        _configuration = configuration;
        _firebaseAuthenticationService = firebaseAuthenticationService;
        _firebaseStorageRepository = firebaseStorageRepository;
    }
    
    public async Task<ResponseModel> Login(LoginRequestModel? login)
    {
        if (login == null)
            return new ResponseModel("error", 400, "Invalid request data.", null);

        (User? user, string idToken, string refreshToken) signInResult;

        try
        {
            if (!string.IsNullOrEmpty(login.IdToken))
            {
                signInResult = await _authRepository.HandleGoogleSignInWithTokensAsync(login.IdToken);
            }
            else if (!string.IsNullOrEmpty(login.Email) && !string.IsNullOrEmpty(login.Password))
            {
                signInResult = await _authRepository.HandleEmailPasswordSignInWithTokensAsync(login.Email, login.Password);
            }
            else
            {
                return new ResponseModel("error", 400, "Invalid login request. Provide either IdToken or Email/Password.", null);
            }
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, ex.Message, null);
        }

        if (signInResult.user == null)
            return new ResponseModel("error", 404, "User not found in the system.", null);

        var session = new UserSession
        {
            UserId = signInResult.user.UserId,
            AccessToken = signInResult.idToken,
            RefreshToken = signInResult.refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _repository.UserSessionRepository.CreateAsync(session);
        }
        catch (Exception dbEx)
        {
            return new ResponseModel("error", 500, "Failed to create user session.", dbEx.Message);
        }

        var responseModel = _mapper.Map<LoginViewModel>(signInResult.user);
        responseModel.Session = _mapper.Map<SessionViewModel>(session);

        return new ResponseModel("success", 200, "Login successful.", responseModel);
    }

    // Common registration helper method
    private async Task<ResponseModel> RegisterUserAsync(UserRegisterModel? registerModel, int roleValue)
    {
        if (registerModel == null)
            return new ResponseModel("error", 400, "Request data is missing.", null);

        if (string.IsNullOrEmpty(registerModel.FirebaseUid) || string.IsNullOrEmpty(registerModel.Email))
            return new ResponseModel("error", 400, "Firebase UID and Email are required.", null);

        // Validate and upload avatar if provided (reuse the same logic)
        string uploadedUrl = string.Empty;
        if (registerModel.AvatarFile != null)
        {
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(registerModel.AvatarFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                return new ResponseModel("error", 400, "Only JPG, JPEG, or PNG files are allowed.", null);

            try
            {
                var fileName = $"avatars/{Guid.NewGuid()}";
                uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.AvatarFile, "avatars");
                if (string.IsNullOrEmpty(uploadedUrl))
                    return new ResponseModel("error", 500, "Failed to upload avatar.", null);
            }
            catch (Exception ex)
            {
                return new ResponseModel("error", 500, "Error uploading avatar.", ex.Message);
            }
        }

        // Check for duplicate user
        var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
            registerModel.Email, registerModel.FirebaseUid, registerModel.PhoneNumber);
        if (existingUser != null)
            return new ResponseModel("error", 409, "A user with this email, phone, or Firebase UID already exists.", null);

        // Get Firebase user and validate email/phone
        try
        {
            var firebaseUser = await _firebaseAuthenticationService.GetFirebaseUserAsync(registerModel.FirebaseUid);
            if (!firebaseUser.EmailVerified)
                return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
            if (!string.IsNullOrEmpty(registerModel.PhoneNumber))
            {
                if (firebaseUser.PhoneNumber != registerModel.PhoneNumber)
                    return new ResponseModel("error", 400, "Phone number does not match with Firebase record.", null);
                if (!firebaseUser.PhoneNumber.StartsWith("+"))
                    return new ResponseModel("error", 400, "Invalid phone number format. Ensure the number is in E.164 format.", null);
            }
        }
        catch (FirebaseAuthException fbaEx)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", fbaEx.Message);
        }

        // Create the User object
        var newUser = new User
        {
            FirebaseId = registerModel.FirebaseUid,
            Email = registerModel.Email,
            UserName = registerModel.UserName,
            ProfilePicture = uploadedUrl,
            PhoneNumber = registerModel.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Role = BitConverter.GetBytes(roleValue)
        };

        try
        {
            var createdUser = await _repository.UserRepository.CreateAsync(newUser);
            if (createdUser != null)
            {
                int roleInt = BitConverter.ToInt32(createdUser.Role, 0);
                string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";
                var claims = new Dictionary<string, object>
                {
                    { "role", roleName },
                    { "custom_id", createdUser.UserId }
                };
                await _firebaseAuthenticationService.SetCustomClaimsAsync(createdUser.FirebaseId, claims);
            }
            return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newUser));
        }
        catch (Exception dbEx)
        {
            return new ResponseModel("error", 500, "Database error occurred while creating user.", dbEx.Message);
        }
    }

    // Use the helper method for both user and shop registration
    public Task<ResponseModel> UserRegister(UserRegisterModel? registerModel)
    {
        // 256 represents cyclist role
        return RegisterUserAsync(registerModel, 256);
    }

    public async Task<ResponseModel> ShopRegister(ShopOwnerRegisterModel? registerModel)
    {
        if (registerModel == null)
            return new ResponseModel("error", 400, "Request data is missing.", null);

        if (string.IsNullOrEmpty(registerModel.FirebaseUid) || string.IsNullOrEmpty(registerModel.Email))
            return new ResponseModel("error", 400, "Firebase UID and Email are required.", null);

        // Validate and upload avatar if provided (reuse the same logic)
        string uploadedUrl = string.Empty;
        if (registerModel.AvatarFile != null)
        {
            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(registerModel.AvatarFile.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                return new ResponseModel("error", 400, "Only JPG, JPEG, or PNG files are allowed.", null);

            try
            {
                var fileName = $"avatars/{Guid.NewGuid()}";
                uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.AvatarFile, "avatars");
                if (string.IsNullOrEmpty(uploadedUrl))
                    return new ResponseModel("error", 500, "Failed to upload avatar.", null);
            }
            catch (Exception ex)
            {
                return new ResponseModel("error", 500, "Error uploading avatar.", ex.Message);
            }
        }

        // Check for duplicate user
        var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
            registerModel.Email, registerModel.FirebaseUid, registerModel.PhoneNumber);
        if (existingUser != null)
            return new ResponseModel("error", 409, "A user with this email, phone, or Firebase UID already exists.", null);

        // Get Firebase user and validate email/phone
        try
        {
            var firebaseUser = await _firebaseAuthenticationService.GetFirebaseUserAsync(registerModel.FirebaseUid);
            if (!firebaseUser.EmailVerified)
                return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
            if (!string.IsNullOrEmpty(registerModel.PhoneNumber))
            {
                if (firebaseUser.PhoneNumber != registerModel.PhoneNumber)
                    return new ResponseModel("error", 400, "Phone number does not match with Firebase record.", null);
                if (!firebaseUser.PhoneNumber.StartsWith("+"))
                    return new ResponseModel("error", 400, "Invalid phone number format. Ensure the number is in E.164 format.", null);
            }
        }
        catch (FirebaseAuthException fbaEx)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", fbaEx.Message);
        }

        // Create the User object
        var newUser = new User
        {
            FirebaseId = registerModel.FirebaseUid,
            Email = registerModel.Email,
            UserName = registerModel.UserName,
            ProfilePicture = uploadedUrl,
            PhoneNumber = registerModel.PhoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Role = BitConverter.GetBytes(128)
        };

        try
        {
            var createdUser = await _repository.UserRepository.CreateAsync(newUser);
            if (createdUser != null)
            {
                int roleInt = BitConverter.ToInt32(createdUser.Role, 0);
                string roleName = (roleInt == 128) ? "cyclist" : "shop_owner";
                var claims = new Dictionary<string, object>
                {
                    { "role", roleName },
                    { "custom_id", createdUser.UserId }
                };
                await _firebaseAuthenticationService.SetCustomClaimsAsync(createdUser.FirebaseId, claims);
            }
            return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newUser));
        }
        catch (Exception dbEx)
        {
            return new ResponseModel("error", 500, "Database error occurred while creating user.", dbEx.Message);
        }
    }

    
    public async Task<ResponseModel> ResetPassword(string email)
    {
        if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
            return new ResponseModel("error", 400, "Invalid email format.", null);

        try
        {
            var firebaseUser = await _firebaseAuthenticationService.GetFirebaseUserAsync(email);
            if (!firebaseUser.EmailVerified)
                return new ResponseModel("error", 404, "User not found or email not verified in Firebase.", null);
        }
        catch (FirebaseAuthException fbaEx)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", fbaEx.Message);
        }

        var user = await _repository.UserRepository.GetUserByPropertyAsync(email);
        if (user == null)
            return new ResponseModel("error", 404, "User not found in the system.", null);

        var firebaseApiKey = _configuration["Firebase:ApiKey"];
        if (string.IsNullOrEmpty(firebaseApiKey))
            return new ResponseModel("error", 500, "Firebase API Key is missing!", null);

        try
        {
            await _firebaseAuthenticationService.SendPasswordResetEmailAsync(email, firebaseApiKey);
            return new ResponseModel("success", 200, "Password reset email sent successfully.", null);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected server error occurred.", ex.Message);
        }
    }

    public async Task<ResponseModel> SendEmailVerify(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return new ResponseModel("error", 400, "Email is required.", null);
        }

        UserRecord? userRecord = null;
        bool userExists = true;

        // Try to fetch user from Firebase by email.
        try
        {
            var user = await _repository.UserRepository.GetUserByPropertyAsync(email);
            if (user == null) return new ResponseModel("error", 404, "User not found.", null);
            userRecord = await _firebaseAuthenticationService.GetFirebaseUserAsync(user.FirebaseId);
        }
        catch (FirebaseAuthException)
        {
            userExists = false; // User does not exist in Firebase.
        }

        // If the user does not exist, create a new user with a temporary password.
        if (!userExists)
        {
            try
            {
                var createUserArgs = new UserRecordArgs
                {
                    Email = email,
                    Password = "TempPassword123!",
                    EmailVerified = false,
                    Disabled = false
                };
                userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(createUserArgs);
            }
            catch (FirebaseAuthException createEx)
            {
                return new ResponseModel("error", 500, "Failed to create user.", createEx.Message);
            }
        }

        // If the email is already verified, no need to send another verification email.
        if (userRecord != null && userRecord.EmailVerified)
        {
            return new ResponseModel("success", 200, "Email is already verified.", null);
        }

        // Retrieve Firebase API key from configuration.
        var firebaseApiKey = _configuration["Firebase:ApiKey"];
        if (string.IsNullOrEmpty(firebaseApiKey))
        {
            return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
        }

        try
        {
            // Delegate to FirebaseService to send the verification email.
            var verificationResponse = await _firebaseAuthenticationService.SendEmailVerificationAsync(email, "TempPassword123!", firebaseApiKey);
            return new ResponseModel("success", 200, "Verification email sent successfully. Please check your email.", userRecord);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Failed to send verification email.", ex.Message);
        }
    }
    public async Task<ResponseModel> SendPhoneOtp(SendPhoneOtpRequestModel? requestModel)
    {
        if (requestModel == null || string.IsNullOrEmpty(requestModel.PhoneNumber))
        {
            return new ResponseModel("error", 400, "Phone number is required.", null);
        }

        // Validate phone number against E.164 format.
        if (!Regex.IsMatch(requestModel.PhoneNumber, @"^\+?[1-9]\d{1,14}$"))
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
            var otpResponse = await _firebaseAuthenticationService.SendPhoneOtpAsync(requestModel.PhoneNumber, requestModel.RecaptchaToken, firebaseApiKey);
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

    public async Task<ResponseModel> VerifyPhoneOtp(VerifyPhoneOtpRequestModel? requestModel)
    {
        if (requestModel == null || string.IsNullOrEmpty(requestModel.VerificationId) || string.IsNullOrEmpty(requestModel.OtpCode))
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
            var authResponse = await _firebaseAuthenticationService.VerifyPhoneOtpAsync(requestModel.VerificationId, requestModel.OtpCode, firebaseApiKey);
            if (string.IsNullOrEmpty(authResponse?.IdToken))
            {
                return new ResponseModel("error", 401, "Authentication failed. No token received.", null);
            }

            // Verify the token and retrieve the UID.
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(authResponse.IdToken);
            var uid = decodedToken.Uid;

            // Retrieve the Firebase user.
            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
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
*/
