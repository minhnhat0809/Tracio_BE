using System.Text;
using System.Text.Json;
using AutoMapper;
using FirebaseAdmin.Auth;
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
    Task<ResponseModel> ResetPassword(string email);
    Task<ResponseModel> Login( LoginRequestModel loginModel);
    Task<ResponseModel> UserRegister(UserRegisterModel registerModel);
    Task<ResponseModel> ShopRegister(ShopOwnerRegisterModel registerModel);
    Task<ResponseModel> GetUrlAvatar(IFormFile file);
}
public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IFirebaseStorageRepository _firebaseStorageRepository;
    private readonly IUnitOfWork _repository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    public AuthService(
        IAuthRepository authRepository,
        IUnitOfWork repository,
        IMapper mapper,
        IConfiguration configuration,
        IFirebaseStorageRepository firebaseStorageRepository)
    {
        _authRepository = authRepository;
        _repository = repository;
        _mapper = mapper;
        _configuration = configuration;
        _firebaseStorageRepository = firebaseStorageRepository;
    }

    public async Task<ResponseModel> Login(LoginRequestModel login)
    {
        try
        {
            User? user = null;
            string firebaseIdToken = string.Empty;
            string firebaseRefreshToken = string.Empty;

            // Check login type (Google Sign-In or Email/Password)
            if (!string.IsNullOrEmpty(login.IdToken))
            {
                var googleSignInResult = await _authRepository.HandleGoogleSignInWithTokensAsync(login.IdToken);
                user = googleSignInResult.User;
                firebaseIdToken = googleSignInResult.IdToken;
                firebaseRefreshToken = "LoginWithGoogleNotIncludingRefreshToken";
            }
            else if (!string.IsNullOrEmpty(login.Email) && !string.IsNullOrEmpty(login.Password))
            {
                var emailPasswordSignInResult = await _authRepository.HandleEmailPasswordSignInWithTokensAsync(login.Email, login.Password);
                user = emailPasswordSignInResult.User;
                firebaseIdToken = emailPasswordSignInResult.IdToken;
                firebaseRefreshToken = emailPasswordSignInResult.RefreshToken;
            }

            // User not found
            if (user == null)
            {
                return new ResponseModel("error", 404, "User not found in the system.", null);
            }

            // Create user session with tokens
            var session = new UserSession
            {
                UserId = user.UserId,
                AccessToken = firebaseIdToken,
                RefreshToken = firebaseRefreshToken,
                
                ExpiresAt = DateTime.UtcNow.AddHours(2), // Adjust expiration as needed
                CreatedAt = DateTime.UtcNow
            };

            // Save session to database
            await _repository.UserSessionRepository.CreateAsync(session);

            // Map user and session to the response model
            var responseModel = _mapper.Map<LoginViewModel>(user);
            responseModel.Session = _mapper.Map<SessionViewModel>(session);

            return new ResponseModel("success", 200, "Login successful.", responseModel);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Server error.", ex.Message);
        }
    }

    public async Task<ResponseModel> UserRegister(UserRegisterModel registerModel)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(registerModel.FirebaseUid) || string.IsNullOrEmpty(registerModel.Email))
            {
                return new ResponseModel("error", 400, "Firebase UID and Email are required.", null);
            }

            // Check if user already exists
            var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
                registerModel.Email, registerModel.FirebaseUid, registerModel.PhoneNumber
            );

            if (existingUser != null)
            {
                return new ResponseModel("error", 409, "A user with this email, phone, or Firebase ID already exists.", null);
            }

            // Get Firebase User
            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(registerModel.FirebaseUid);
            if (firebaseUser == null)
            {
                return new ResponseModel("error", 404, "A user with this Firebase UID was not found.", null);
            }

            // Add avatar
            string uploadedUrl = "";
            if (registerModel.AvatarFile != null)
            {
                var fileName = $"avatars/{Guid.NewGuid()}"; // Store avatar in 'avatars' folder with a unique name

                uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.AvatarFile, "avatars");

                if (string.IsNullOrEmpty(uploadedUrl))
                {
                    return new ResponseModel("error", 500, "Failed to upload image.", null);
                }
            }

            // Define Cyclist Role as Binary (100000000 = 256 in decimal)
            int cyclistRole = 256;
            byte[] roleBytes = BitConverter.GetBytes(cyclistRole); // Convert int to byte array (Little Endian)

            // Map registration model to User entity
            var newUser = new User
            {
                FirebaseId = registerModel.FirebaseUid, // Firebase UID as User ID
                Email = registerModel.Email,
                UserName = registerModel.DisplayName,
                ProfilePicture = uploadedUrl,
                PhoneNumber = registerModel.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = roleBytes // Assign Cyclist role as byte array
            };

            // Create User in Database
            await _repository.UserRepository.CreateAsync(newUser);

            return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newUser));
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, $"Internal server error: {ex.Message}", ex.StackTrace);
        }
    }


    public async Task<ResponseModel> ShopRegister(ShopOwnerRegisterModel registerModel)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(registerModel.FirebaseUid) || string.IsNullOrEmpty(registerModel.Email))
            {
                return new ResponseModel("error", 400, "Firebase UID and Email are required.", null);
            }

            // Check if user already exists
            var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
                registerModel.Email, registerModel.FirebaseUid, registerModel.PhoneNumber
            );

            if (existingUser != null)
            {
                return new ResponseModel("error", 409, "A user with this email, phone, or Firebase ID already exists.", null);
            }

            // Get Firebase User
            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(registerModel.FirebaseUid);
            if (firebaseUser == null)
            {
                return new ResponseModel("error", 404, "A user with this Firebase UID was not found.", null);
            }
            
            // Add avatar
            var uploadedUrl= string.Empty;
            if (registerModel.AvatarFile != null)
            {
                var fileName = $"avatars/{Guid.NewGuid()}"; // Store avatar in 'avatars' folder with a unique name

                uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.AvatarFile, "avatars");

                if (string.IsNullOrEmpty(uploadedUrl))
                {
                    return new ResponseModel("error", 500, "Failed to upload image.", null);
                }
            }

            // Define ShopOwner Role as Binary ( 128 in decimal)
            int shopOwnerRole = 128;
            byte[] roleBytes = BitConverter.GetBytes(shopOwnerRole); // Convert int to byte array (Little Endian)
            
            // Map registration model to User entity
            var newShopOwner = new User
            {
                FirebaseId= registerModel.FirebaseUid, // Firebase UID as User ID
                Email = registerModel.Email,
                UserName = registerModel.DisplayName,
                ProfilePicture = uploadedUrl,
                PhoneNumber = registerModel.PhoneNumber,
                //ProviderId = registerModel.ProviderId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = roleBytes // Assign a valid role
            };

            // Create User in Database
            await _repository.UserRepository.CreateAsync(newShopOwner);
    

            return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newShopOwner));
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, $"Internal server error: {ex.Message}", ex.StackTrace);
        }
    }
    
    public async Task<ResponseModel> ResetPassword(string email)
    {
        try
        {
            // Get Firebase User
            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            var user = await _repository.UserRepository.GetUserByPropertyAsync(email);

            if (user == null)
            {
                return new ResponseModel("error", 404, "User not found in the system.", null);
            }

            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
            }

            var resetPasswordUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";

            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email = email
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(resetPasswordUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Failed to send password reset email.", responseBody);
            }

            return new ResponseModel("success", 200, "Password reset email sent successfully.", null);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Server error.", ex.Message);
        }
    }

    
    public async Task<ResponseModel> GetUrlAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new ResponseModel("error", 400, "No file uploaded.", null);
            }

            var fileName = $"avatars/{Guid.NewGuid()}"; // Store avatar in 'avatars' folder with a unique name
            string uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, file, "avatars");

            if (string.IsNullOrEmpty(uploadedUrl))
            {
                return new ResponseModel("error", 500, "Failed to upload image.", null);
            }

            return new ResponseModel("success", 201, "Avatar uploaded successfully.", new { url = uploadedUrl });
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, $"Internal server error: {ex.Message}", null);
        }
    }
    
}