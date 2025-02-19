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
    Task<ResponseModel> VerifyPhoneNumberLinkWithCredential(VerifyPhoneNumberLinkRequestModel? requestModel);

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

    public async Task<ResponseModel> Login(LoginRequestModel? login)
    {
        try
        {
            if (login == null)
            {
                return new ResponseModel("error", 400, "Invalid request data.", null);
            }

            User? user = null;
            string firebaseIdToken;
            string firebaseRefreshToken;

            // Validate login method
            if (!string.IsNullOrEmpty(login.IdToken))
            {
                try
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
                catch (Exception authEx)
                {
                    return new ResponseModel("error", 500, "Google authentication error.", authEx.Message);
                }
            }
            else if (!string.IsNullOrEmpty(login.Email) && !string.IsNullOrEmpty(login.Password))
            {
                try
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
                catch (Exception authEx)
                {
                    return new ResponseModel("error", 500, "Email/password authentication error.", authEx.Message);
                }
            }
            else
            {
                return new ResponseModel("error", 400, "Invalid login request. Provide either IdToken or Email/Password.", null);
            }

            // User not found
            if (user == null)
            {
                return new ResponseModel("error", 404, "User not found in the system.", null);
            }

            // session
            var session = new UserSession
            {
                UserId = user.UserId,
                AccessToken = firebaseIdToken,
                RefreshToken = firebaseRefreshToken,
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

            // Response model
            var responseModel = _mapper.Map<LoginViewModel>(user);
            responseModel.Session = _mapper.Map<SessionViewModel>(session);

            return new ResponseModel("success", 200, "Login successful.", responseModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return new ResponseModel("error", 500, "Server error.", "An unexpected error occurred.");
        }
    }


    public async Task<ResponseModel> UserRegister(UserRegisterModel? registerModel)
    {
        try
        {
            // Validate required fields
            if (registerModel == null)
            {
                return new ResponseModel("error", 400, "Request data is missing.", null);
            }

            if (string.IsNullOrEmpty(registerModel.FirebaseUid))
            {
                return new ResponseModel("error", 400, "Firebase UID is required.", null);
            }

            if (string.IsNullOrEmpty(registerModel.Email))
            {
                return new ResponseModel("error", 400, "Email is required.", null);
            }
            
            if (registerModel.AvatarFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(registerModel.AvatarFile.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                {
                    return new ResponseModel("error", 400, "File Error: Only JPG, JPEG, or PNG files are allowed.", null);
                }

            }

            // Check if the user already exists
            var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
                registerModel.Email, registerModel.FirebaseUid, registerModel.PhoneNumber
            );

            if (existingUser != null)
            {
                return new ResponseModel("error", 409, "A user with this email, phone, or Firebase UID already exists.", null);
            }

            // Get Firebase User
            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(registerModel.FirebaseUid);
                if (firebaseUser == null)
                {
                    return new ResponseModel("error", 404, "No user found in Firebase with the given UID.", null);
                }
                // check email verify
                if (!firebaseUser.EmailVerified)
                {
                    return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
                }
                // check phone verify
                if (!string.IsNullOrEmpty(registerModel.PhoneNumber))
                {
                    if (firebaseUser.PhoneNumber != registerModel.PhoneNumber)
                    {
                        return new ResponseModel("error", 400, "Phone number does not match with Firebase record.", null);
                    }

                    if (!firebaseUser.PhoneNumber.StartsWith("+")) // Kiểm tra định dạng E.164
                    {
                        return new ResponseModel("error", 400, "Invalid phone number format. Ensure the number is in E.164 format.", null);
                    }
                }
            }
            catch (FirebaseAuthException firebaseEx)
            {
                return new ResponseModel("error", 500, "Firebase authentication error.", firebaseEx.Message);
            }

            
            
            // Upload avatar if provided
            string uploadedUrl = "";
            if (registerModel.AvatarFile != null)
            {
                try
                {
                    var fileName = $"avatars/{Guid.NewGuid()}"; // Unique filename
                    uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.AvatarFile, "avatars");

                    if (string.IsNullOrEmpty(uploadedUrl))
                    {
                        return new ResponseModel("error", 500, "Failed to upload avatar.", null);
                    }
                }
                catch (Exception storageEx)
                {
                    return new ResponseModel("error", 500, "Error uploading avatar.", storageEx.Message);
                }
            }

            // Define Cyclist Role as Binary (100000000 = 256 in decimal)
            int cyclistRole = 256;
            byte[] roleBytes = BitConverter.GetBytes(cyclistRole);

            // Create a new User object
            var newUser = new User
            {
                FirebaseId = registerModel.FirebaseUid,
                Email = registerModel.Email,
                UserName = registerModel.UserName,
                ProfilePicture = uploadedUrl,
                PhoneNumber = registerModel.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = roleBytes
            };

            // Create the User in the Database
            try
            {
                var user = await _repository.UserRepository.CreateAsync(newUser);
                
                if (user != null)
                {
                    // Convert role from byte[] to int
                    int roleInt = BitConverter.ToInt32(user.Role, 0);
                    string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";

                    var claims = new Dictionary<string, object>
                    {
                        { "role", roleName },
                        { "custom_id", user.UserId }
                    };
                    await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.FirebaseId, claims);
                }

                return new ResponseModel("success", 201, "User created successfully.", _mapper.Map<UserViewModel>(newUser));
            }
            catch (Exception dbEx)
            {
                return new ResponseModel("error", 500, "Database error occurred while creating user.", dbEx.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UserRegister] Unexpected error: {ex.Message}");

            return new ResponseModel("error", 500, "An unexpected error occurred.", "Internal Server Error.");
        }
    }


    public async Task<ResponseModel> ShopRegister(ShopOwnerRegisterModel? registerModel)
    {
        try
        {
            // Validate required fields
            if (registerModel == null)
            {
                return new ResponseModel("error", 400, "Request data is missing.", null);
            }

            if (string.IsNullOrEmpty(registerModel.FirebaseUid))
            {
                return new ResponseModel("error", 400, "Firebase UID is required.", null);
            }

            if (string.IsNullOrEmpty(registerModel.Email))
            {
                return new ResponseModel("error", 400, "Email is required.", null);
            }
            
            if (registerModel.AvatarFile != null)
            {
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(registerModel.AvatarFile.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
                {
                    return new ResponseModel("error", 400, "File Error: Only JPG, JPEG, or PNG files are allowed.", null);
                }

            }

            // Check if the user already exists
            var existingUser = await _repository.UserRepository.GetUserByMultiplePropertiesAsync(
                registerModel.Email, registerModel.FirebaseUid, registerModel.PhoneNumber
            );

            if (existingUser != null)
            {
                return new ResponseModel("error", 409, "A user with this email, phone, or Firebase UID already exists.", null);
            }

            // Get Firebase User
            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(registerModel.FirebaseUid);
                if (firebaseUser == null)
                {
                    return new ResponseModel("error", 404, "No user found in Firebase with the given UID.", null);
                }
                // check email verify
                if (!firebaseUser.EmailVerified)
                {
                    return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
                }
                // check phone verify
                if (!string.IsNullOrEmpty(registerModel.PhoneNumber))
                {
                    if (firebaseUser.PhoneNumber != registerModel.PhoneNumber)
                    {
                        return new ResponseModel("error", 400, "Phone number does not match with Firebase record.", null);
                    }

                    if (!firebaseUser.PhoneNumber.StartsWith("+")) // Kiểm tra định dạng E.164
                    {
                        return new ResponseModel("error", 400, "Invalid phone number format. Ensure the number is in E.164 format.", null);
                    }
                }
            }
            catch (FirebaseAuthException firebaseEx)
            {
                return new ResponseModel("error", 500, "Firebase authentication error.", firebaseEx.Message);
            }

            // Upload avatar if provided
            string uploadedUrl = "";
            if (registerModel.AvatarFile != null)
            {
                try
                {
                    var fileName = $"avatars/{Guid.NewGuid()}"; // Unique filename
                    uploadedUrl = await _firebaseStorageRepository.UploadImageAsync(fileName, registerModel.AvatarFile, "avatars");

                    if (string.IsNullOrEmpty(uploadedUrl))
                    {
                        return new ResponseModel("error", 500, "Failed to upload avatar.", null);
                    }
                }
                catch (Exception storageEx)
                {
                    return new ResponseModel("error", 500, "Error uploading avatar.", storageEx.Message);
                }
            }

            // Define ShopOwner Role as Binary (128 in decimal)
            int shopOwnerRole = 128;
            byte[] roleBytes = BitConverter.GetBytes(shopOwnerRole);

            // Create a new User object
            var newShopOwner = new User
            {
                FirebaseId = registerModel.FirebaseUid,
                Email = registerModel.Email,
                UserName = registerModel.UserName,
                ProfilePicture = uploadedUrl,
                PhoneNumber = registerModel.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = roleBytes
            };

            // Create the User in the Database
            try
            {
                var shop = await _repository.UserRepository.CreateAsync(newShopOwner);
                
                if (shop != null)
                {
                    // Convert role from byte[] to int
                    int roleInt = BitConverter.ToInt32(shop.Role, 0);
                    string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";

                    var claims = new Dictionary<string, object>
                    {
                        { "role", roleName },
                        { "custom_id", shop.UserId }
                    };
                    await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(shop.FirebaseId, claims);
                }

                return new ResponseModel("success", 201, "Shop owner registered successfully.", _mapper.Map<UserViewModel>(newShopOwner));
            }
            catch (Exception dbEx)
            {
                return new ResponseModel("error", 500, "Database error occurred while creating shop owner.", dbEx.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShopRegister] Unexpected error: {ex.Message}");
            return new ResponseModel("error", 500, "An unexpected error occurred.", "Internal Server Error.");
        }
    }

    public async Task<ResponseModel> ResetPassword(string email)
    {
        try
        {
            // Validate email input
            if (string.IsNullOrEmpty(email) || !new EmailAddressAttribute().IsValid(email))
            {
                return new ResponseModel("error", 400, "Invalid email format.", null);
            }

            // Get Firebase User
            try
            {
                var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
                if (firebaseUser == null)
                {
                    return new ResponseModel("error", 404, "User not found in Firebase.", null);
                }
                // Check Verify
                if (!firebaseUser.EmailVerified)
                {
                    return new ResponseModel("error", 404, "Email is not verified. Please verify your email before registering.", null);
                }
            }
            catch (FirebaseAuthException firebaseEx)
            {
                return new ResponseModel("error", 500, "Firebase authentication error.", firebaseEx.Message);
            }

            // Check if user exists in local database
            var user = await _repository.UserRepository.GetUserByPropertyAsync(email);
            if (user == null)
            {
                return new ResponseModel("error", 404, "User not found in the system.", null);
            }

            // Get Firebase API Key
            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
            }

            // Firebase Password Reset URL
            var resetPasswordUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";

            // Payload for Firebase password reset request
            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email = email
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsync(resetPasswordUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ResponseModel("error", (int)response.StatusCode, "Failed to send password reset email.", responseBody);
                }
            }
            catch (HttpRequestException httpEx)
            {
                return new ResponseModel("error", 500, "Network error while sending reset email.", httpEx.Message);
            }

            return new ResponseModel("success", 200, "Password reset email sent successfully.", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ResetPassword] Unexpected error: {ex.Message}");
            return new ResponseModel("error", 500, "An unexpected server error occurred.", "Internal Server Error.");
        }
    }

    public async Task<ResponseModel> SendEmailVerify(string? email)
    {
        try
        {
            if (string.IsNullOrEmpty(email))
            {
                return new ResponseModel("error", 400, "Email is required.", null);
            }

            UserRecord? user = null;
            bool userExists = true;

            // Try to fetch user from Firebase
            try
            {
                user = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
            }
            catch (FirebaseAuthException)
            {
                userExists = false; // User does not exist
            }

            // If user does not exist, create a new one with a temporary password
            if (!userExists)
            {
                try
                {
                    var createUserArgs = new UserRecordArgs
                    {
                        Email = email,
                        Password = "TempPassword123!", // Temporary password
                        EmailVerified = false,
                        Disabled = false
                    };

                    user = await FirebaseAuth.DefaultInstance.CreateUserAsync(createUserArgs);
                }
                catch (FirebaseAuthException createEx)
                {
                    return new ResponseModel("error", 500, "Failed to create user.", createEx.Message);
                }
            }

            // If email is already verified, no need to send another verification email
            if (user.EmailVerified)
            {
                return new ResponseModel("success", 200, "Email is already verified.", null);
            }

            var firebaseApiKey = _configuration["Firebase:ApiKey"];

            // Firebase API URL for sending email verification
            var verifyEmailUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={firebaseApiKey}";

            // Get IdToken from Email/Password
            var userResponse = await _authRepository.HandleEmailPasswordSignInWithTokensAsync(email, "TempPassword123!");
            
            // Payload for sending verification email
            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken = userResponse.IdToken,
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(verifyEmailUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Failed to send verification email.", responseBody);
            }

            return new ResponseModel("success", 200, "Verification email sent successfully. Please check your email.", 
                user);
        }
        catch (FirebaseAuthException ex)
        {
            return new ResponseModel("error", 500, "Firebase authentication error.", ex.Message);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }

    
    public async Task<ResponseModel> SendPhoneOtp(SendPhoneOtpRequestModel? requestModel)
    {
        try
        {
            if (requestModel == null || string.IsNullOrEmpty(requestModel.PhoneNumber))
            {
                return new ResponseModel("error", 400, "Phone number is required.", null);
            }

            if (!Regex.IsMatch(requestModel.PhoneNumber, @"^\+?[1-9]\d{1,14}$")) // E.164 format check
            {
                return new ResponseModel("error", 400, "Invalid phone number format.", null);
            }

            var firebaseApiKey = _configuration["Firebase:ApiKey"];

            // Firebase REST API for sending OTP
            var sendOtpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:sendVerificationCode?key={firebaseApiKey}";

            var payload = new
            {
                phoneNumber = requestModel.PhoneNumber,
                recaptchaToken = requestModel.RecaptchaToken
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            var response = await client.PostAsync(sendOtpUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Failed to send OTP.", responseBody);
            }

            // Extract VerificationId (sessionInfo) from Firebase response
            var result = JsonSerializer.Deserialize<FirebaseSendOtpResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (string.IsNullOrEmpty(result?.SessionInfo))
            {
                return new ResponseModel("error", 500, "Failed to generate verification ID.", null);
            }

            return new ResponseModel("success", 200, "OTP sent successfully.", new
            {
                VerificationId = result.SessionInfo
            });
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }


    public async Task<ResponseModel> VerifyPhoneOtp(VerifyPhoneOtpRequestModel? requestModel)
{
    try
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

        // Firebase API URL to verify OTP
        var verifyOtpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={firebaseApiKey}";

        // Payload to verify OTP
        var payload = new
        {
            sessionInfo = requestModel.VerificationId, // The verification ID from frontend
            code = requestModel.OtpCode // The OTP code entered by the user
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using var client = new HttpClient();
        var response = await client.PostAsync(verifyOtpUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new ResponseModel("error", (int)response.StatusCode, "Invalid OTP.", responseBody);
        }

        // Extract idToken from response
        var result = JsonSerializer.Deserialize<FirebaseAuthResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (string.IsNullOrEmpty(result?.IdToken))
        {
            return new ResponseModel("error", 401, "Authentication failed. No token received.", null);
        }

        // Get user info from Firebase
        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(result.IdToken);
        var uid = decodedToken.Uid;

        var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
        if (firebaseUser == null)
        {
            return new ResponseModel("error", 404, "User not found.", null);
        }

        // Check if user exists in the database
        var user = await _repository.UserRepository.GetUserByPropertyAsync(uid);
        if (user == null)
        {
            return new ResponseModel("error", 404, "User not found in Database", null);
        }

        // Create session for the user
        var session = new UserSession
        {
            UserId = user.UserId,
            AccessToken = result.IdToken,
            RefreshToken = result.RefreshToken,
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

    public async Task<ResponseModel> VerifyPhoneNumberLinkWithCredential(
        VerifyPhoneNumberLinkRequestModel? requestModel)
    {
        try
        {
            if (requestModel == null || string.IsNullOrEmpty(requestModel.VerificationId) ||
                string.IsNullOrEmpty(requestModel.OtpCode))
            {
                return new ResponseModel("error", 400, "Verification ID and OTP are required.", null);
            }

            var firebaseApiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(firebaseApiKey))
            {
                return new ResponseModel("error", 500, "Firebase API Key is missing!", null);
            }

            // Firebase API URL to verify OTP
            var verifyOtpUrl =
                $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={firebaseApiKey}";

            // Payload to verify OTP
            var payload = new
            {
                sessionInfo = requestModel.VerificationId, // The verification ID from frontend
                code = requestModel.OtpCode // The OTP code entered by the user
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            // do verify phone otp
            using var client = new HttpClient();
            var response = await client.PostAsync(verifyOtpUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)response.StatusCode, "Invalid OTP.", responseBody);
            }

            var result = JsonSerializer.Deserialize<FirebaseAuthResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            if (string.IsNullOrEmpty(result?.IdToken))
            {
                return new ResponseModel("error", 401, "Authentication failed. No token received.", null);
            }

            var linkPhoneUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={firebaseApiKey}";
            var linkPayload = new
            {
                idToken = result.IdToken, // Authenticated user's ID token
                phoneNumber = requestModel.PhoneNumber, // Phone number to link
                returnSecureToken = true
            };

            var jsonLinkPayload = JsonSerializer.Serialize(linkPayload);
            var linkContent = new StringContent(jsonLinkPayload, Encoding.UTF8, "application/json");
            // link phone to user credential
            var linkResponse = await client.PostAsync(linkPhoneUrl, linkContent);
            var linkResponseBody = await linkResponse.Content.ReadAsStringAsync();

            if (!linkResponse.IsSuccessStatusCode)
            {
                return new ResponseModel("error", (int)linkResponse.StatusCode, "Failed to link phone number.",
                    linkResponseBody);
            }

            /*var linkResult = JsonSerializer.Deserialize<FirebaseAuthResponse>(linkResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (string.IsNullOrEmpty(linkResult?.IdToken))
            {
                return new ResponseModel("error", 500,
                    "Failed to obtain updated user token after linking phone number.", null);
            }
            */

            return new ResponseModel("success", 200, "Phone number linked successfully.", linkResponseBody);
        }
        catch (Exception ex)
        {
            return new ResponseModel("error", 500, "Unexpected error occurred.", ex.Message);
        }
    }

}