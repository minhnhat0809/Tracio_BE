using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using UserService.Application.Interfaces;
using User = UserService.Domain.Entities.User;

namespace UserService.Infrastructure.Repositories;



public class AuthRepository : IAuthRepository
{
    private readonly IUnitOfWork _repository;
    private readonly IConfiguration _configuration;
    public AuthRepository(IUnitOfWork repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<(User? User, string IdToken, string RefreshToken)> HandleGoogleSignInWithTokensAsync(string idToken)
    {
        try
        {
            // ✅ Xác thực ID Token với Firebase Admin SDK
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string uid = decodedToken.Uid;

            // ✅ Lấy user từ Firebase
            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
            if (firebaseUser == null)
            {
                return (null, string.Empty, string.Empty);
            }

            // ✅ Lấy user từ hệ thống (CSDL)
            var user = await _repository.UserRepository.GetUserByPropertyAsync(firebaseUser.Email);
            if (user != null)
            {
                // Chuyển đổi role từ byte[] sang string
                int roleInt = BitConverter.ToInt32(user.Role, 0);
                string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";

                // Gán custom claims
                var claims = new Dictionary<string, object>
                {
                    { "role", roleName },
                    { "custom_id", user.UserId }
                };
                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.FirebaseId, claims);
                
            }
            else
            {
                return (null, string.Empty, string.Empty);
            }

            // ✅ Tạo Firebase Custom Token để trả về cho client
            //string customToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);

            return (user, idToken, ""); // Firebase Admin SDK không tạo refresh token
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

        try
        {
            var authResult = await authProvider.SignInWithEmailAndPasswordAsync(email, password);
            
            // Fetch the user from your system
            var user = await _repository.UserRepository.GetUserByPropertyAsync(email);
            if (user != null)
            {
                int roleInt = BitConverter.ToInt32(user.Role, 0);
                string roleName = (roleInt == 256) ? "cyclist" : "shop_owner";
                var claims = new Dictionary<string, object>
                {
                    { "role", roleName },
                    { "custom_id", user.UserId }
                };

                await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(user.FirebaseId, claims);
            }
            // Return the authenticated user along with Firebase tokens
            return (await _repository.UserRepository.GetUserByPropertyAsync(email), authResult.User.Credential.IdToken, authResult.User.Credential.RefreshToken);
        }
        catch (Firebase.Auth.FirebaseAuthException)
        {
            return (null, string.Empty, string.Empty);
        }
    }
}