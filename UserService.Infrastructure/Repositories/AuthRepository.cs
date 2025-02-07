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

    public async Task<(User? User, string IdToken)> HandleGoogleSignInWithTokensAsync(string idToken)
    {
        // Verify the ID token with Firebase Admin SDK
        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        string uid = decodedToken.Uid;
        var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

        if (firebaseUser == null)
        {
            return (null, string.Empty); // Return null user and no tokens
        }

        // Fetch the user from your system
        var user = await _repository.UserRepository.GetUserByPropertyAsync(firebaseUser.Email);

        return (user, idToken); // Return ID token and no refresh token
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

            // Return the authenticated user along with Firebase tokens
            return (await _repository.UserRepository.GetUserByPropertyAsync(email), authResult.User.Credential.IdToken, authResult.User.Credential.RefreshToken);
        }
        catch (Firebase.Auth.FirebaseAuthException)
        {
            return (null, string.Empty, string.Empty);
        }
    }
}